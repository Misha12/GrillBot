using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Entity.Users;
using Grillbot.Database.Enums.Includes;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models;
using Grillbot.Models.Invites;
using Grillbot.Services.Initiable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DBDiscordUser = Grillbot.Database.Entity.Users.DiscordUser;

namespace Grillbot.Services.InviteTracker
{
    public class InviteTrackerService : IInitiable
    {
        private DiscordSocketClient Discord { get; }
        private BotState BotState { get; }
        private ILogger<InviteTrackerService> Logger { get; }
        private IConfiguration Configuration { get; }
        private SearchService SearchService { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        public InviteTrackerService(DiscordSocketClient discord, BotState botState, ILogger<InviteTrackerService> logger,
            IConfiguration configuration, SearchService searchService, IGrillBotRepository grillBotRepository)
        {
            Discord = discord;
            BotState = botState;
            Logger = logger;
            Configuration = configuration;
            SearchService = searchService;
            GrillBotRepository = grillBotRepository;
        }

        public void Init()
        {
        }

        public async Task InitAsync()
        {
            await RefreshInvitesAsync();
        }

        public async Task<string> RefreshInvitesAsync()
        {
            var invites = await GetLatestInvitesAwait();
            Logger.LogInformation($"Invite tracker loaded. Loaded invites: {invites.Sum(o => o.Value.Count)} on {invites.Count} guild{(invites.Count > 1 ? "s" : "")}");

            UpdateInvites(invites);

            return $"Pozvánky obnoveny.\nPozvánek: **{invites.Sum(o => o.Value.Count).FormatWithSpaces()}**\nPočet serverů: **{invites.Count.FormatWithSpaces()}**";
        }

        private void UpdateInvites(Dictionary<ulong, List<InviteModel>> invites)
        {
            if (BotState.InviteCache.Count > 0)
                BotState.InviteCache.Clear();

            BotState.InviteCache.AddRange(invites);
        }

        public async Task<Dictionary<ulong, List<InviteModel>>> GetLatestInvitesAwait()
        {
            var invites = new Dictionary<ulong, List<InviteModel>>();

            foreach (var guild in Discord.Guilds)
            {
                var guildInvites = await GetLatestInvitesOfGuildAsync(guild);

                if (guildInvites.Count > 0)
                    invites.Add(guild.Id, guildInvites);
            }

            return invites;
        }

        public async Task<List<InviteModel>> GetLatestInvitesOfGuildAsync(SocketGuild guild)
        {
            var resultInvites = new List<InviteModel>();

            var vanityUrlInvite = string.IsNullOrEmpty(guild.VanityURLCode) ? null : await guild.GetVanityInviteAsync();
            var vanityUses = await GetVanityUrlUsesAsync(guild);

            var guildInvites = await guild.GetInvitesAsync();

            if (guildInvites.Count > 0)
                resultInvites.AddRange(guildInvites.Select(o => new InviteModel(o)));

            if (vanityUrlInvite != null)
            {
                var vanityModel = new InviteModel(vanityUrlInvite)
                {
                    Uses = vanityUses
                };

                resultInvites.Add(vanityModel);
            }

            return resultInvites;
        }

        private async Task<int?> GetVanityUrlUsesAsync(SocketGuild guild)
        {
            if (string.IsNullOrEmpty(guild.VanityURLCode))
                return null;

            using var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(global::Discord.DiscordConfig.APIUrl)
            };

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bot {Configuration["Token"]}");

            var vanityData = await httpClient.GetAsync($"guilds/{guild.Id}/vanity-url");

            if (!vanityData.IsSuccessStatusCode)
                return null;

            var content = await vanityData.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            return json["uses"]?.Value<int?>();
        }

        public async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            if (!user.IsUser())
                return;

            var latestInvites = await GetLatestInvitesOfGuildAsync(user.Guild);
            var usedInvite = FindUsedInvite(user.Guild, latestInvites);

            if (usedInvite == null)
            {
                Logger.LogWarning($"User {user.GetFullName()} ({user.Id}) user unkown invite.");
                return;
            }

            DBDiscordUser inviteCreator = null;
            if (usedInvite.Creator != null)
            {
                inviteCreator = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(user.Guild.Id, usedInvite.Creator.Id, UsersIncludes.Invites);
                await GrillBotRepository.CommitAsync();
            }

            var inviteEntity = await GrillBotRepository.InviteRepository.FindInviteAsync(usedInvite.Code);

            if (inviteEntity == null)
            {
                inviteEntity = new Invite()
                {
                    ChannelIdSnowflake = usedInvite.ChannelId,
                    Code = usedInvite.Code,
                    CreatedAt = usedInvite.CreatedAt.HasValue ? usedInvite.CreatedAt.Value.UtcDateTime : (DateTime?)null,
                    CreatorId = inviteCreator?.ID
                };

                await GrillBotRepository.AddAsync(inviteEntity);
            }

            var joinedUser = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(user.Guild.Id, user.Id, UsersIncludes.Invites);
            joinedUser.UsedInviteCode = usedInvite.Code;

            await GrillBotRepository.CommitAsync();
            AppendInvites(user.Guild, latestInvites);
        }

        private InviteModel FindUsedInvite(SocketGuild guild, List<InviteModel> latestInvites)
        {
            if (!BotState.InviteCache.ContainsKey(guild.Id))
                return null;

            var guildInvites = BotState.InviteCache[guild.Id];

            // Different count of invite use.
            var diffInvite = guildInvites.Find(invite =>
                {
                    var latestInvite = latestInvites.Find(x => x.Code == invite.Code);
                    return latestInvite != null && latestInvite.Uses > invite.Uses;
                });

            return diffInvite ?? latestInvites.Find(invite => !guildInvites.Any(x => x.Code == invite.Code));
        }

        public async Task AssignInviteToUserAsync(SocketUser user, SocketGuild guild, string code)
        {
            var latestInvites = await GetLatestInvitesOfGuildAsync(guild);
            var usedInvite = latestInvites.Find(o => o.Code == code);

            if (usedInvite == null)
                throw new NotFoundException($"Pozvánka s kódem `{code}` neexistuje");

            DBDiscordUser inviteCreator = null;
            if (usedInvite.Creator != null)
            {
                inviteCreator = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, usedInvite.Creator.Id, UsersIncludes.Invites);
                await GrillBotRepository.CommitAsync();
            }

            var inviteEntity = await GrillBotRepository.InviteRepository.FindInviteAsync(usedInvite.Code);

            if (inviteEntity == null)
            {
                inviteEntity = new Invite()
                {
                    ChannelIdSnowflake = usedInvite.ChannelId,
                    Code = usedInvite.Code,
                    CreatedAt = usedInvite.CreatedAt.HasValue ? usedInvite.CreatedAt.Value.UtcDateTime : (DateTime?)null,
                    CreatorId = inviteCreator?.ID
                };

                await GrillBotRepository.AddAsync(inviteEntity);
            }

            var userEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, user.Id, UsersIncludes.Invites);
            userEntity.UsedInviteCode = code;

            await GrillBotRepository.CommitAsync();
            AppendInvites(guild, latestInvites);
        }

        public async Task<List<InviteModel>> GetStoredInvitesAsync(InvitesListFilter filter)
        {
            var skip = (filter.Page == 0 ? 0 : filter.Page - 1) * PaginationInfo.DefaultPageSize;
            var query = (await GetInvitesQueryAsync(filter)).Skip(skip).Take(PaginationInfo.DefaultPageSize);
            var guild = Discord.GetGuild(filter.GuildID);

            var invites = await query.ToListAsync();
            var result = new List<InviteModel>();
            foreach (var invite in invites)
            {
                var isVanity = invite.Code == guild.VanityURLCode;

                if (invite.Creator == null)
                {
                    result.Add(new InviteModel(invite, null, isVanity, invite.UsedUsers.Count));
                    continue;
                }

                var creator = await guild.GetUserFromGuildAsync(invite.Creator.UserIDSnowflake);
                result.Add(new InviteModel(invite, creator, isVanity, invite.UsedUsers.Count));
            }

            return result;
        }

        public async Task<PaginationInfo> GetPaginationInfoAsync(InvitesListFilter filter)
        {
            var query = await GetInvitesQueryAsync(filter);
            var count = await query.CountAsync();

            var skip = (filter.Page == 0 ? 0 : filter.Page - 1) * PaginationInfo.DefaultPageSize;
            return new PaginationInfo(skip, filter.Page, count);
        }

        private async Task<IQueryable<Invite>> GetInvitesQueryAsync(InvitesListFilter filter)
        {
            var guild = Discord.GetGuild(filter.GuildID);
            var usersFromQuery = await SearchService.FindUsersAsync(guild, filter.UserQuery);
            var userIds = usersFromQuery != null ? (await SearchService.ConvertUsersToIDsAsync(usersFromQuery)).Select(o => o.Value).Where(o => o != null).Select(o => o.Value).ToList() : null;

            return GrillBotRepository.InviteRepository.GetInvitesQuery(filter.GuildID, filter.CreatedFrom, filter.CreatedTo, userIds, filter.Desc);
        }

        private void AppendInvites(SocketGuild guild, List<InviteModel> invites)
        {
            if (!BotState.InviteCache.ContainsKey(guild.Id))
            {
                BotState.InviteCache.Add(guild.Id, invites);
            }
            else
            {
                BotState.InviteCache[guild.Id].Clear();
                BotState.InviteCache[guild.Id].AddRange(invites);
            }
        }
    }
}
