using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WatchDog_Bot.Services
{
    public class EmoteChain
    {
        private Dictionary<ulong, List<string>> LastMessages { get; }
        private int ReactLimit { get; }

        public EmoteChain(IConfigurationRoot configuration)
        {
            ReactLimit = Convert.ToInt32(configuration["EmoteChain:CheckLastN"]);
            LastMessages = new Dictionary<ulong, List<string>>();
        }

        public void Cleanup(ISocketMessageChannel channel)
        {
            if (!LastMessages.ContainsKey(channel.Id)) return;
            LastMessages[channel.Id].Clear();
        }

        public async Task ProcessChain(SocketCommandContext context)
        {
            var content = context.Message.Content;
            var channel = context.Channel;

            if (!LastMessages.ContainsKey(channel.Id))
                LastMessages.Add(channel.Id, new List<string>(ReactLimit));

            if (!IsEmote(content) || !IsLocalEmote(context))
            {
                Cleanup(channel);
                return;
            }

            LastMessages[channel.Id].Add(content);
            await TryReact(channel);
        }

        private async Task TryReact(ISocketMessageChannel channel)
        {
            if(LastMessages[channel.Id].Count == ReactLimit)
            {
                await channel.SendMessageAsync(LastMessages[channel.Id][0]);
            }
        }

        private bool IsEmote(string content)
        {
            if (!content.StartsWith("<:")) return false;
            if (!content.EndsWith(">")) return false;
            if (content.Contains(" ")) return false;

            return content.Count(c => c == ':') == 2;
        }

        private bool IsLocalEmote(SocketCommandContext context)
        {
            return context.Guild.Emotes.Any(o => o.ToString() == context.Message.Content);
        }
    }
}
