using Discord.WebSocket;
using Grillbot.Database.Entity.Users;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Services.Initiable;
using Grillbot.Services.InviteTracker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grillbot.Services
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
            var invites = await GetLatestInvitesAwait();
            Logger.LogInformation($"Invite tracker loaded. Loaded invites: {invites.Sum(o => o.Value.Count)} on {invites.Count} guild{(invites.Count > 1 ? "s" : "")}");

            UpdateInvites(invites);
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

            DiscordUser inviteCreator = null;
            if (usedInvite.Creator != null)
            {
                inviteCreator = UsersRepository.GetOrCreateUser(user.Guild.Id, usedInvite.Creator.Id, false, false, false, false, false, true);
                UsersRepository.SaveChangesIfAny();
            }

            InviteRepository.StoreInviteIfNotExists(usedInvite, inviteCreator);

            var joinedUser = UsersRepository.GetOrCreateUser(user.Guild.Id, user.Id, false, false, false, false, false, true);
            joinedUser.UsedInviteCode = usedInvite.Code;

            InviteRepository.SaveChanges();
            UsersRepository.SaveChanges();

            BotState.InviteCache[user.Guild.Id].Clear();
            BotState.InviteCache[user.Guild.Id].AddRange(latestInvites);
        }

        private InviteModel FindUsedInvite(SocketGuild guild, List<InviteModel> latestInvites)
        {
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

        public void Dispose()
        {
            UsersRepository.Dispose();
            InviteRepository.Dispose();
        }
    }
}
