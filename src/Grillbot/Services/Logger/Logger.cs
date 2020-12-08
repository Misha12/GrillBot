using Discord.WebSocket;
using Grillbot.Services.Config;
using Grillbot.Services.Logger.LoggerMethods;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.Logger
{
    public class Logger : IDisposable
    {
        private DiscordSocketClient Client { get; }
        private ILogger<Logger> AppLogger { get; }
        private ConfigurationService ConfigurationService { get; }
        public Dictionary<string, uint> Counters { get; }

        private DateTime LastEventAt { get; set; }
        private string LastEvent { get; set; }
        private readonly object LastEventLock = new object();

        public Logger(DiscordSocketClient client, IMessageCache messageCache, ILogger<Logger> logger, ConfigurationService configurationService)
        {
            Client = client;
            AppLogger = logger;
            ConfigurationService = configurationService;

            Counters = new Dictionary<string, uint>();
        }

        public async Task OnGuildMemberUpdatedAsync(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            if (!CanProcessEvent("GuildMemberUpdated")) return;

            var method = new GuildMemberUpdated(Client, ConfigurationService);
            var result = await method.ProcessAsync(guildUserBefore, guildUserAfter).ConfigureAwait(false);

            if (result)
                EventPostProcess("GuildMemberUpdated");
        }

        private void EventPostProcess(string name)
        {
            AppLogger.LogInformation($"Logger event {name} triggered.");

            if (!Counters.ContainsKey(name))
                Counters.Add(name, 1);
            else
                Counters[name]++;

            lock (LastEventLock)
            {
                LastEvent = name;
                LastEventAt = DateTime.UtcNow;
            }
        }

        public void Dispose()
        {
            Counters.Clear();
        }

        private bool CanProcessEvent(string @event)
        {
            lock (LastEventLock)
            {
                return @event != LastEvent || (DateTime.UtcNow - LastEventAt).TotalSeconds > 1;
            }
        }
    }
}
