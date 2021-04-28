using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class EmoteChain
    {
        // Dictionary<GuildID|ChannelID, List<UserID, Message>>
        private Dictionary<string, List<Tuple<ulong, string>>> LastMessages { get; }
        private int ReactLimit { get; }

        private readonly object Locker = new object();

        public EmoteChain(IConfiguration configuration)
        {
            var reactLimit = configuration["EmoteChain_CheckCount"];
            ReactLimit = string.IsNullOrEmpty(reactLimit) ? -1 : Convert.ToInt32(reactLimit);

            LastMessages = new Dictionary<string, List<Tuple<ulong, string>>>();
        }

        public void Cleanup(SocketGuildChannel channel)
        {
            lock (Locker)
            {
                CleanupNoLock(channel);
            }
        }

        public void CleanupNoLock(SocketGuildChannel channel)
        {
            var key = GetKey(channel);

            if (LastMessages.ContainsKey(key))
            {
                LastMessages[key].Clear();
            }
        }

        public async Task ProcessChainAsync(SocketCommandContext context)
        {
            if (context.Channel is not SocketTextChannel channel || ReactLimit == -1) return;

            var author = context.Message.Author;
            var content = context.Message.Content;
            var key = GetKey(channel);

            if (!LastMessages.ContainsKey(key))
                LastMessages.Add(key, new List<Tuple<ulong, string>>(ReactLimit));

            if (!IsValidMessage(context.Message, context.Guild, channel))
            {
                CleanupNoLock(channel);
                return;
            }

            var group = LastMessages[key];

            if (!group.Any(o => o.Item1 == author.Id))
                group.Add(new Tuple<ulong, string>(author.Id, content));

            if (group.Count == ReactLimit)
            {
                await channel.SendMessageAsync(group[0].Item2);
                CleanupNoLock(channel);
            }
        }

        private bool IsValidWithWithFirstInChannel(SocketGuildChannel channel, string content)
        {
            var key = GetKey(channel);
            var group = LastMessages[key];

            if (group.Count == 0)
                return true;

            return content == group[0].Item2;
        }

        private bool IsValidMessage(SocketUserMessage message, SocketGuild guild, SocketGuildChannel channel)
        {
            var emotes = message.Tags
               .Where(o => o.Type == TagType.Emoji && guild.Emotes.Any(x => x.Id == o.Key))
               .ToList();

            var isUTFEmoji = NeoSmart.Unicode.Emoji.IsEmoji(message.Content);
            if (emotes.Count == 0 && !isUTFEmoji) return false;

            if (!IsValidWithWithFirstInChannel(channel, message.Content)) return false;
            var emoteTemplate = string.Join(" ", emotes.Select(o => o.Value.ToString()));
            return emoteTemplate == message.Content || isUTFEmoji;
        }

        private string GetKey(SocketGuildChannel channel)
        {
            return $"{channel.Guild.Id}|{channel.Id}";
        }
    }
}
