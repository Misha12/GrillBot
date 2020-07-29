using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Services.Initiable;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class InviteTrackerService : IInitiable
    {
        private DiscordSocketClient Discord { get; }
        private BotState BotState { get; }
        private ILogger<InviteTrackerService> Logger { get; }

        public InviteTrackerService(DiscordSocketClient discord, BotState botState, ILogger<InviteTrackerService> logger)
        {
            Discord = discord;
            BotState = botState;
            Logger = logger;
        }

        public void Init()
        {
        }

        public async Task InitAsync()
        {
            var invites = new Dictionary<ulong, List<RestInviteMetadata>>();

            foreach (var guild in Discord.Guilds)
            {
                var guildInvites = await guild.GetInvitesAsync();

                if (guildInvites.Count == 0)
                    continue;

                invites.Add(guild.Id, guildInvites.ToList());
            }

            Logger.LogInformation($"Invite tracker loaded. Loaded invites: {invites.Sum(o => o.Value.Count)} on {invites.Count} guild{(invites.Count > 1 ? "s" : "")}");

            if (BotState.InviteCache.Count > 0)
                BotState.InviteCache.Clear();

            BotState.InviteCache.AddRange(invites);
        }
    }
}
