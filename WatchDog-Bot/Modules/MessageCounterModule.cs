using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchDog_Bot.Extensions;
using WatchDog_Bot.Repository;

namespace WatchDog_Bot.Modules
{
    public class MessageCounterModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// Statistics cache.
        /// On start is loaded from database. 
        /// Every N minutes and on bot shutdown data are saved to database.
        /// </summary>
        private Dictionary<ulong, Dictionary<ulong, ulong>> Counters { get; }
        private static object CountersLock { get; } = new object();

        private IServiceProvider Services { get; }
        private string CommandPrefix { get; }

        public MessageCounterModule(IConfigurationRoot config, IServiceProvider services)
        {
            CommandPrefix = config["CommandPrefix"];
            Services = services;

            Counters = new Dictionary<ulong, Dictionary<ulong, ulong>>();
        }

        public async Task Increment(SocketMessage message)
        {
            if (message.Content.StartsWith(CommandPrefix)) return;
            await Task.Run(() => IncrementValue(message.Author.Id, message.Channel.Id));
        }

        public async Task Decrement(Cacheable<IMessage, ulong> oldMessage, ISocketMessageChannel channel)
        {
            if (!oldMessage.HasValue) return;

            var channelID = channel.Id;
            if (!Counters.ContainsKey(channelID)) return;

            var authorID = oldMessage.Value.Author.Id;
            if (!Counters[channelID].ContainsKey(authorID)) return;

            await Task.Run(() =>
            {
                lock (CountersLock)
                {
                    Counters[channelID][authorID]--;
                }
            });
        }

        public async Task BulkDecrement(IReadOnlyCollection<Cacheable<IMessage, ulong>> oldMessages, ISocketMessageChannel channel)
        {
            foreach (var message in oldMessages)
            {
                await Decrement(message, channel);
            }
        }

        private void IncrementValue(ulong authorID, ulong channelID)
        {
            lock (CountersLock)
            {
                if (!Counters.ContainsKey(channelID))
                {
                    Counters.Add(channelID, new Dictionary<ulong, ulong>() { { authorID, 1 } });
                }
                else
                {
                    var channel = Counters[channelID];

                    if (!channel.ContainsKey(authorID))
                    {
                        channel.Add(authorID, 1);
                    }
                    else
                    {
                        channel[authorID]++;
                    }
                }
            }
        }

        [Command("messageboard")]
        private async Task GetUserStats()
        {
            using (var repository = (MessagesCounterRepository)Services.GetService(typeof(MessagesCounterRepository)))
            {
                await GetPerChannelStats(repository);
                await GetPerUserStats(repository);
            }
        }

        private async Task GetPerChannelStats(MessagesCounterRepository repository)
        {
            var embed = new EmbedBuilder()
            {
                Color = Color.Green,
                Description = "**Počty zpráv dle místností**"
            };

            await ReplyAsync("", false, embed.Build());
        }

        private async Task GetPerUserStats(MessagesCounterRepository repository)
        {
            var embed = new EmbedBuilder()
            {
                Color = Color.Orange,
                Description = "**Počty zpráv dle uživatelů (TOP 10)**"
            };


            var users = await repository.GetUsersMessageCountAsync(10, false);
            foreach(var user in users)
            {
                var guildUser = Context.Guild.GetUser(user.Item1);

                embed.AddField(o =>
                {
                    o.Name = guildUser.Username;
                    o.Value = user.Item2.Format("#,0");
                });
            }

            await ReplyAsync("", false, embed.Build());
        }
    }
}
