using Discord.WebSocket;
using Grillbot.Database.Enums.Includes;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Models.Users;
using Grillbot.Services.Initiable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DBDiscordUser = Grillbot.Database.Entity.Users.DiscordUser;

namespace Grillbot.Services.InviteTracker
{
    public class InviteTrackerService : IInitiable, IDisposable
    {
        private DiscordSocketClient Discord { get; }
        private BotState BotState { get; }
        private ILogger<InviteTrackerService> Logger { get; }
        private Configuration Config { get; }
        private UsersRepository UsersRepository { get; }
        private InviteRepository InviteRepository { get; }

        public InviteTrackerService(DiscordSocketClient discord, BotState botState, ILogger<InviteTrackerService> logger,
            IOptions<Configuration> config, UsersRepository usersRepository, InviteRepository inviteRepository)
        {
            Discord = discord;
            BotState = botState;
            Logger = logger;
            Config = config.Value;
            UsersRepository = usersRepository;
            InviteRepository = inviteRepository;
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

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bot {Config.Discord.Token}");

            var vanityData = await httpClient.GetAsync($"guilds/{guild.Id}/vanity-url");

            if (!vanityData.IsSuccessStatusCode)
                return null;

            var content = await vanityData.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            return json["uses"]?.Value<int?>();
        }

        public async Task OnUserJoinedAsync(SocketGuildUser user)
        {
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
                inviteCreator = UsersRepository.GetOrCreateUser(user.Guild.Id, usedInvite.Creator.Id, UsersIncludes.Invites);
                UsersRepository.SaveChangesIfAny();
            }

            InviteRepository.StoreInviteIfNotExists(usedInvite, inviteCreator);

            var joinedUser = UsersRepository.GetOrCreateUser(user.Guild.Id, user.Id, UsersIncludes.Invites);
            joinedUser.UsedInviteCode = usedInvite.Code;

            InviteRepository.SaveChanges();
            UsersRepository.SaveChanges();

            BotState.InviteCache[user.Guild.Id].Clear();
            BotState.InviteCache[user.Guild.Id].AddRange(latestInvites);
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

            if (diffInvite != null)
                return diffInvite;

            return latestInvites.Find(invite => !guildInvites.Any(x => x.Code == invite.Code));
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
                inviteCreator = UsersRepository.GetOrCreateUser(guild.Id, usedInvite.Creator.Id, UsersIncludes.Invites);
                UsersRepository.SaveChangesIfAny();
            }

            InviteRepository.StoreInviteIfNotExists(usedInvite, inviteCreator);

            var userEntity = UsersRepository.GetOrCreateUser(guild.Id, user.Id, UsersIncludes.Invites);
            userEntity.UsedInviteCode = code;

            InviteRepository.SaveChanges();
            UsersRepository.SaveChanges();

            BotState.InviteCache[guild.Id].Clear();
            BotState.InviteCache[guild.Id].AddRange(latestInvites);
        }

        public async Task<List<DiscordUser>> GetUsersWithCodeAsync(SocketGuild guild, string code)
        {
            var users = await UsersRepository.GetUsersWithUsedCode(guild.Id, code).ToListAsync();

            var result = new List<DiscordUser>();
            foreach (var user in users)
            {
                result.Add(await UserHelper.MapUserAsync(Discord, BotState, user));
            }

            return result;
        }

        public async Task<List<InviteModel>> GetStoredInvitesAsync(SocketGuild guild)
        {
            var invites = await InviteRepository.GetInvitesAsync(guild, true, false);
            var result = new List<InviteModel>();

            foreach (var invite in invites.Where(o => o != null))
            {
                if (invite.Creator == null)
                {
                    result.Add(new InviteModel(invite, null, invite.UsedUsers.Count));
                    continue;
                }

                var creator = await guild.GetUserFromGuildAsync(invite.Creator.UserIDSnowflake);
                result.Add(new InviteModel(invite, creator, invite.UsedUsers.Count));
            }

            return result;
        }

        public void Dispose()
        {
            UsersRepository.Dispose();
            InviteRepository.Dispose();
        }
    }
}
