using Discord.WebSocket;
using Grillbot.Extensions;
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
    public class InviteTrackerService : IInitiable
    {
        private DiscordSocketClient Discord { get; }
        private BotState BotState { get; }
        private ILogger<InviteTrackerService> Logger { get; }
        private Configuration Config { get; }

        public InviteTrackerService(DiscordSocketClient discord, BotState botState, ILogger<InviteTrackerService> logger,
            IOptions<Configuration> config)
        {
            Discord = discord;
            BotState = botState;
            Logger = logger;
            Config = config.Value;
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
                    UsesCount = vanityUses
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
    }
}
