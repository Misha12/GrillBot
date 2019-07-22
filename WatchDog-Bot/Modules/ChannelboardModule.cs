using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchDog_Bot.Helpers;
using WatchDog_Bot.Services;
using WatchDog_Bot.Services.Statistics;

namespace WatchDog_Bot.Modules
{
    [Name("Channel leaderboards")]
    public class ChannelboardModule : BotModuleBase
    {
        private Statistics Statistics { get; }
        private int TakeTop { get; }

        public ChannelboardModule(Statistics statistics, IConfigurationRoot configuration)
        {
            Statistics = statistics;
            TakeTop = Convert.ToInt32(configuration["Leaderboards:ChannelStatsTakeTop"]);
        }

        [Command("channelboard")]
        [RequireRole(RoleGroupName = "Channelboard")]
        public async Task Channelboard()
        {
            await Channelboard(TakeTop);
        }

        [Command("channelboard")]
        [Remarks("Možnost zvolit TOP N kanálů.")]
        [RequireRole(RoleGroupName = "Channelboard")]
        public async Task Channelboard(int takeTop)
        {
            var channelBoardData = Statistics.ChannelCounter
                .OrderByDescending(o => o.Value)
                .Where(o => CanAuthorToChannel(o.Key))
                .Take(takeTop).ToList();

            var messageBuilder = new StringBuilder()
                .AppendLine("=======================")
                .AppendLine("|\tCHANNEL LEADERBOARD\t|")
                .AppendLine("=======================");

            for(int i = 0; i < channelBoardData.Count; i++)
            {
                var channelBoardItem = channelBoardData[i];
                var channel = Context.Client.GetChannel(channelBoardItem.Key) as IMessageChannel;

                messageBuilder
                    .Append(i + 1).Append(": ").Append(channel.Name)
                    .Append(" - ").Append(FormatHelper.FormatWithSpaces(channelBoardItem.Value))
                    .AppendLine();
            }

            await ReplyAsync(messageBuilder.ToString());
        }

        private bool CanAuthorToChannel(ulong channelID)
        {
            var channel = Context.Client.GetChannel(channelID);

            if (channel == null) return false;
            return channel.Users.Any(o => o.Id == Context.Message.Author.Id);
        }
    }
}
