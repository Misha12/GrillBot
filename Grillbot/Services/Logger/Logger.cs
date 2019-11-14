using Discord;
using Discord.WebSocket;
using Grillbot.Models;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Logger.LoggerMethods;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.Options;
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
        private Configuration Config { get; }
        private IMessageCache MessageCache { get; }
        private BotLoggingService LoggingService { get; }

        public Dictionary<string, TopStack> EventsTopStack { get; }
        public Dictionary<string, uint> Counters { get; }

        public Logger(DiscordSocketClient client, IOptions<Configuration> config, IMessageCache messageCache, BotLoggingService loggingService)
        {
            Client = client;
            Config = config.Value;
            MessageCache = messageCache;
            LoggingService = loggingService;

            Counters = new Dictionary<string, uint>();
            EventsTopStack = new Dictionary<string, TopStack>();

            HttpClient = new HttpClient();
        }

        public async Task OnGuildMemberUpdatedAsync(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            var stack = GetTopStack("GuildMemberUpdated");
            var method = new GuildMemberUpdated(Client, Config, stack);
            var result = await method.ProcessAsync(guildUserBefore, guildUserAfter);

            if (result)
                IncrementEventHandle("GuildMemberUpdated");
        }

        public async Task OnMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var stack = GetTopStack("MessageDeleted");
            var method = new MessageDeleted(Client, Config, MessageCache, HttpClient, LoggingService, stack);
            await method.ProcessAsync(message, channel);

            IncrementEventHandle("MessageDeleted");
        }

        public async Task OnMessageEdited(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            var stack = GetTopStack("MessageEdited");
            var method = new MessageEdited(Client, Config, MessageCache, stack);
            var result = await method.ProcessAsync(messageBefore, messageAfter, channel);

            if (result)
                IncrementEventHandle("MessageEdited");
        }

        public async Task OnUserJoined(SocketGuildUser user)
        {
            var stack = GetTopStack("UserJoined");
            var method = new UserJoined(Client, Config, stack);
            await method.ProcessAsync(user);

            IncrementEventHandle("UserJoined");
        }

        public async Task OnUserLeft(SocketGuildUser user)
        {
            var stack = GetTopStack("UserLeft");
            var method = new UserLeft(Client, Config, stack);
            await method.ProcessAsync(user);

            IncrementEventHandle("UserLeft");
        }

        private void IncrementEventHandle(string name)
        {
            if (!Counters.ContainsKey(name))
                Counters.Add(name, 1);
            else
                Counters[name]++;
        }

        public TopStack GetTopStack(string eventName, bool createNew = true)
        {
            if (!EventsTopStack.ContainsKey(eventName))
            {
                if (!createNew)
                    return null;

                var stack = new TopStack();
                EventsTopStack.Add(eventName, stack);

                return stack;
            }

            return EventsTopStack[eventName];
        }

        public void Dispose()
        {
            HttpClient.Dispose();
            Counters.Clear();

            foreach (var stack in EventsTopStack)
            {
                stack.Value.Clear();
            }

            EventsTopStack.Clear();
        }
    }
}
