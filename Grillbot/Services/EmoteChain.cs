using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class EmoteChain : IConfigChangeable
    {
        // Dictionary<ChannelID, List<UserID, Message>>
        private Dictionary<ulong, List<Tuple<ulong, string>>> LastMessages { get; }
        private int ReactLimit { get; set; }
        private SemaphoreSlim Semaphore { get; }

        public EmoteChain(IOptions<Configuration> configuration)
        {
            ReactLimit = configuration.Value.EmoteChain_CheckLastCount;
            LastMessages = new Dictionary<ulong, List<Tuple<ulong, string>>>();
            Semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task CleanupAsync(ISocketMessageChannel channel, bool @lock = false)
        {
            if (@lock)
                await Semaphore.WaitAsync();

            try
            {
                if (!LastMessages.ContainsKey(channel.Id)) return;
                LastMessages[channel.Id].Clear();
            }
            finally
            {
                if (@lock)
                    Semaphore.Release();
            }
        }

        public async Task ProcessChainAsync(SocketCommandContext context)
        {
            await Semaphore.WaitAsync();

            try
            {
                var author = context.Message.Author;
                var content = context.Message.Content;
                var channel = context.Channel;

                if (!LastMessages.ContainsKey(channel.Id))
                    LastMessages.Add(channel.Id, new List<Tuple<ulong, string>>(ReactLimit));

                if (!IsValidMessage(context))
                {
                    await CleanupAsync(channel);
                    return;
                }

                if (!LastMessages[channel.Id].Any(o => o.Item1 == author.Id))
                {
                    LastMessages[channel.Id].Add(new Tuple<ulong, string>(author.Id, content));
                }

                await TryReactAsync(channel);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private async Task TryReactAsync(ISocketMessageChannel channel)
        {
            if(LastMessages[channel.Id].Count == ReactLimit)
            {
                await channel.SendMessageAsync(LastMessages[channel.Id][0].Item2);
                await CleanupAsync(channel);
            }
        }

        private bool IsValidWithWithFirstInChannel(SocketCommandContext context)
        {
            var channel = LastMessages[context.Channel.Id];

            if (channel.Count == 0)
                return true;

            return context.Message.Content == channel[0].Item2;
        }

        private bool IsValidMessage(SocketCommandContext context)
        {
            var emotes = context.Message.Tags
               .Where(o => o.Type == TagType.Emoji && context.Guild.Emotes.Any(x => x.Id == o.Key))
               .ToList();

            if (emotes.Count == 0) return false;

            var emoteTemplate = string.Join(" ", emotes.Select(o => o.Value.ToString()));
            return emoteTemplate == context.Message.Content && IsValidWithWithFirstInChannel(context);
        }

        public void ConfigChanged(IConfiguration newConfig)
        {
            //TODO
            Semaphore.Wait();

            try
            {
                ReactLimit = Convert.ToInt32(newConfig["EmoteChain:CheckLastN"]);
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}
