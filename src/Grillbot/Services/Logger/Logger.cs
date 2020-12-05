using Discord;
using Discord.WebSocket;
using Grillbot.Services.Config;
using Grillbot.Services.Logger.LoggerMethods;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grillbot.Services.Logger
{
    public class Logger : IDisposable
    {
        private HttpClient HttpClient { get; }
        private DiscordSocketClient Client { get; }
        private IMessageCache MessageCache { get; }
        private ILogger<Logger> AppLogger { get; }
        private ConfigurationService ConfigurationService { get; }
        public Dictionary<string, uint> Counters { get; }

        private DateTime LastEventAt { get; set; }
        private string LastEvent { get; set; }
        private readonly object LastEventLock = new object();

        public Logger(DiscordSocketClient client, IMessageCache messageCache, ILogger<Logger> logger, ConfigurationService configurationService)
        {
            Client = client;
            MessageCache = messageCache;
            AppLogger = logger;
            ConfigurationService = configurationService;

            Counters = new Dictionary<string, uint>();

            HttpClient = new HttpClient();
        }

        public async Task OnGuildMemberUpdatedAsync(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            if (!CanProcessEvent("GuildMemberUpdated")) return;

            var method = new GuildMemberUpdated(Client, ConfigurationService);
            var result = await method.ProcessAsync(guildUserBefore, guildUserAfter).ConfigureAwait(false);

            if (result)
                EventPostProcess("GuildMemberUpdated");
        }

        public async Task OnMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (!CanProcessEvent("MessageDeleted")) return;

            var method = new MessageDeleted(Client, ConfigurationService, MessageCache, HttpClient, AppLogger);
            await method.ProcessAsync(message, channel).ConfigureAwait(false);

            EventPostProcess("MessageDeleted");
        }

        public async Task OnMessageEdited(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            if (!CanProcessEvent("MessageEdited")) return;

            var method = new MessageEdited(Client, ConfigurationService, MessageCache);
            var result = await method.ProcessAsync(messageBefore, messageAfter, channel).ConfigureAwait(false);

            if (result)
                EventPostProcess("MessageEdited");
        }

        public async Task OnUserJoined(SocketGuildUser user)
        {
            if (!CanProcessEvent("UserJoined")) return;

            var method = new UserJoined(Client, ConfigurationService);
            await method.ProcessAsync(user).ConfigureAwait(false);

            EventPostProcess("UserJoined");
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
            HttpClient.Dispose();
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
