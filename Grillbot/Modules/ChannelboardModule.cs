using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grillbot.Helpers;
#pragma warning disable CS0234 // The type or namespace name 'Services' does not exist in the namespace 'Grillbot' (are you missing an assembly reference?)
using Grillbot.Services;
#pragma warning restore CS0234 // The type or namespace name 'Services' does not exist in the namespace 'Grillbot' (are you missing an assembly reference?)
#pragma warning disable CS0234 // The type or namespace name 'Services' does not exist in the namespace 'Grillbot' (are you missing an assembly reference?)
using Grillbot.Services.Statistics;
#pragma warning restore CS0234 // The type or namespace name 'Services' does not exist in the namespace 'Grillbot' (are you missing an assembly reference?)

namespace Grillbot.Modules
{
    [Name("Channel leaderboards")]
    public class ChannelboardModule : BotModuleBase
    {
#pragma warning disable CS0246 // The type or namespace name 'ChannelStats' could not be found (are you missing a using directive or an assembly reference?)
        private ChannelStats Stats { get; }
#pragma warning restore CS0246 // The type or namespace name 'ChannelStats' could not be found (are you missing a using directive or an assembly reference?)
        private int TakeTop { get; }

#pragma warning disable CS0246 // The type or namespace name 'Statistics' could not be found (are you missing a using directive or an assembly reference?)
        public ChannelboardModule(Statistics statistics, IConfiguration configuration)
#pragma warning restore CS0246 // The type or namespace name 'Statistics' could not be found (are you missing a using directive or an assembly reference?)
        {
            Stats = statistics.ChannelStats;
            TakeTop = Convert.ToInt32(configuration["Leaderboards:ChannelStatsTakeTop"]);
        }

        [Command("channelboard")]
#pragma warning disable CS0246 // The type or namespace name 'RequireRoleAttribute' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning disable CS0246 // The type or namespace name 'RequireRole' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning disable CS0246 // The type or namespace name 'RoleGroupName' could not be found (are you missing a using directive or an assembly reference?)
        [RequireRole(RoleGroupName = "Channelboard")]
#pragma warning restore CS0246 // The type or namespace name 'RoleGroupName' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning restore CS0246 // The type or namespace name 'RequireRole' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning restore CS0246 // The type or namespace name 'RequireRoleAttribute' could not be found (are you missing a using directive or an assembly reference?)
        public async Task Channelboard()
        {
            await Channelboard(TakeTop);
        }

        [Command("channelboard")]
        [Remarks("Možnost zvolit TOP N kanálů.")]
#pragma warning disable CS0246 // The type or namespace name 'RequireRoleAttribute' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning disable CS0246 // The type or namespace name 'RequireRole' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning disable CS0246 // The type or namespace name 'RoleGroupName' could not be found (are you missing a using directive or an assembly reference?)
        [RequireRole(RoleGroupName = "Channelboard")]
#pragma warning restore CS0246 // The type or namespace name 'RoleGroupName' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning restore CS0246 // The type or namespace name 'RequireRole' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning restore CS0246 // The type or namespace name 'RequireRoleAttribute' could not be found (are you missing a using directive or an assembly reference?)
        public async Task Channelboard(int takeTop)
        {
            var channelBoardData = Stats.Counter
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

        [Command("channelboardweb")]
        [Summary("Webový leaderboard.")]
#pragma warning disable CS0246 // The type or namespace name 'RequireRoleAttribute' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning disable CS0246 // The type or namespace name 'RequireRole' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning disable CS0246 // The type or namespace name 'RoleGroupName' could not be found (are you missing a using directive or an assembly reference?)
        [RequireRole(RoleGroupName = "Channelboard")]
#pragma warning restore CS0246 // The type or namespace name 'RoleGroupName' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning restore CS0246 // The type or namespace name 'RequireRole' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning restore CS0246 // The type or namespace name 'RequireRoleAttribute' could not be found (are you missing a using directive or an assembly reference?)
        public async Task ChannelboardWeb()
        {
            var token = Stats.CreateWebToken(Context);

            var message = $"Tady máš odkaz na channelboard serveru **{Context.Guild.Name}**: {token.Url}. Odkaz platí do *{token.GetExpirationDate()}*";
            await Context.Message.Author.SendMessageAsync(message);
        }

        [Command("channelboard")]
        [Summary("Počet zpráv v místnosti.")]
#pragma warning disable CS0246 // The type or namespace name 'RequireRoleAttribute' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning disable CS0246 // The type or namespace name 'RequireRole' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning disable CS0246 // The type or namespace name 'RoleGroupName' could not be found (are you missing a using directive or an assembly reference?)
        [RequireRole(RoleGroupName = "Channelboard")]
#pragma warning restore CS0246 // The type or namespace name 'RoleGroupName' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning restore CS0246 // The type or namespace name 'RequireRole' could not be found (are you missing a using directive or an assembly reference?)
#pragma warning restore CS0246 // The type or namespace name 'RequireRoleAttribute' could not be found (are you missing a using directive or an assembly reference?)
        public async Task ChannelboardForRoom(string roomMention)
        {
            var channel = Context.Guild.Channels.FirstOrDefault(o => $"<#{o.Id}>" == roomMention);

            if (!CanAuthorToChannel(channel.Id))
                await Context.Message.Author.SendMessageAsync("Do této místnosti nemáš dostatečná práva.");

            var value = Stats.GetValue(channel.Id);
            var formatedMessageCount = FormatHelper.FormatWithSpaces(value.Item2);
            var message = $"Aktuální počet zpráv v místnosti **{channel.Name}** je **{formatedMessageCount}** a v příčce se drží na **{value.Item1}**. pozici.";

            await Context.Message.Author.SendMessageAsync(message);
            await Context.Message.DeleteAsync();
        }
    }
}
