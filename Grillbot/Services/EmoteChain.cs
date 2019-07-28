using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Grillbot.Services
{
#pragma warning disable CS0246 // The type or namespace name 'IConfigChangeable' could not be found (are you missing a using directive or an assembly reference?)
    public class EmoteChain : IConfigChangeable
#pragma warning restore CS0246 // The type or namespace name 'IConfigChangeable' could not be found (are you missing a using directive or an assembly reference?)
    {
        // Dictionary<ChannelID, List<UserID, Message>>
        private Dictionary<ulong, List<Tuple<ulong, string>>> LastMessages { get; }
        private int ReactLimit { get; set; }
        private SemaphoreSlim Semaphore { get; }

        public EmoteChain(IConfiguration configuration)
        {
            ReactLimit = Convert.ToInt32(configuration["EmoteChain:CheckLastN"]);
            LastMessages = new Dictionary<ulong, List<Tuple<ulong, string>>>();
            Semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task Cleanup(ISocketMessageChannel channel, bool @lock = false)
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

        public async Task ProcessChain(SocketCommandContext context)
        {
            await Semaphore.WaitAsync();

            try
            {
                var author = context.Message.Author;
                var content = context.Message.Content;
                var channel = context.Channel;

                if (!IsValidMessage(context))
                {
                    await Cleanup(channel);
                    return;
                }

                if (!LastMessages.ContainsKey(channel.Id))
                    LastMessages.Add(channel.Id, new List<Tuple<ulong, string>>(ReactLimit));

                if (!LastMessages[channel.Id].Any(o => o.Item1 == author.Id))
                {
                    LastMessages[channel.Id].Add(new Tuple<ulong, string>(author.Id, content));
                }

                await TryReact(channel);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private async Task TryReact(ISocketMessageChannel channel)
        {
            if(LastMessages[channel.Id].Count == ReactLimit)
            {
                await channel.SendMessageAsync(LastMessages[channel.Id][0].Item2);
            }
        }

        private bool IsEmote(SocketCommandContext context)
        {
            return Regex.IsMatch(context.Message.Content, @"<:[^:\s]*(?:::[^:\s]*)*:\d+>");
        }

        private bool IsLocalEmote(SocketCommandContext context)
        {
            return context.Guild.Emotes.Any(o => o.ToString() == context.Message.Content);
        }

        private bool IsValidMessage(SocketCommandContext context) => IsEmote(context) && IsLocalEmote(context);

        public void ConfigChanged(IConfiguration newConfig)
        {
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
