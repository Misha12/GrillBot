using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grillbot.Helpers;
using Grillbot.Services.Statistics;
using Grillbot.Services.Preconditions;
using Discord.WebSocket;
using Grillbot.Exceptions;
using Newtonsoft.Json;
using Grillbot.Extensions.Discord;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Name("Channel leaderboards")]
    public class ChannelboardModule : BotModuleBase
    {
        private ChannelStats Stats { get; }

        public ChannelboardModule(ChannelStats channelStats)
        {
            Stats = channelStats;
        }

        [Command("channelboard")]
        public async Task ChannelboardAsync() => await ChannelboardAsync(ChannelStats.ChannelboardTakeTop).ConfigureAwait(false);

        [Command("channelboard")]
        [Remarks("Možnost zvolit TOP N kanálů.")]
        public async Task ChannelboardAsync(int takeTop)
        {
            var channelboardData = Stats.GetAllValues()
                .Where(o => CanAuthorToChannel(o.SnowflakeID))
                .Take(takeTop)
                .ToList();

            var messageBuilder = new StringBuilder()
                .AppendLine("=======================")
                .AppendLine("|\tCHANNEL LEADERBOARD\t|")
                .AppendLine("=======================");

            for (int i = 0; i < channelboardData.Count; i++)
            {
                var channelBoardItem = channelboardData[i];
                var channel = Context.Client.GetChannel(channelBoardItem.SnowflakeID) as IMessageChannel;

                messageBuilder
                    .Append(i + 1).Append(": ").Append(channel.Name)
                    .Append(" - ").AppendLine(FormatHelper.FormatWithSpaces(channelBoardItem.Count));
            }

            await Context.Message.Author.SendPrivateMessageAsync(messageBuilder.ToString()).ConfigureAwait(false);
        }

        private bool CanAuthorToChannel(ulong channelID)
        {
            var channel = Context.Client.GetChannel(channelID);
            if (channel is IPrivateChannel) return false;

            return channel?.Users.Any(o => o.Id == Context.Message.Author.Id) ?? false;
        }

        [Command("channelboardweb")]
        [Summary("Webový leaderboard.")]
        public async Task ChannelboardWebAsync()
        {
            var token = Stats.CreateWebToken(Context);

            var message = $"Tady máš odkaz na channelboard serveru **{Context.Guild.Name}**: {token.Url}. Odkaz platí do *{token.GetExpirationDate()}*";
            await Context.Message.Author.SendPrivateMessageAsync(message).ConfigureAwait(false);
        }

        [Command("channelboard")]
        [Summary("Počet zpráv v místnosti.")]
        public async Task ChannelboardForRoomAsync(string roomMention)
        {
            await DoAsync(async () =>
            {
                if (Context.Message.Tags.Count == 0)
                    throw new ArgumentException("Nic jsi netagnul.");

                var channelMention = Context.Message.Tags.FirstOrDefault(o => o.Type == TagType.ChannelMention);
                if (channelMention == null)
                    throw new ArgumentException("Netagnul jsi žádný kanál");

                if (!(channelMention.Value is ISocketMessageChannel channel))
                    throw new BotException($"Discord.NET uznal, že je to ChannelMention, ale nepovedlo se mi to načíst jako kanál. Prověřte to někdo pls. (Message: {Context.Message.Content}, Tags: {JsonConvert.SerializeObject(Context.Message.Tags)})");

                if (!CanAuthorToChannel(channel.Id))
                    throw new ArgumentException("Do této místnosti nemáš dostatečná práva.");

                var value = Stats.GetValue(channel.Id);
                var formatedMessageCount = FormatHelper.FormatWithSpaces(value.Item2);
                var message = $"Aktuální počet zpráv v místnosti **{channel.Name}** je **{formatedMessageCount}** a v příčce se drží na **{value.Item1}**. pozici.";

                await Context.Message.Author.SendMessageAsync(message).ConfigureAwait(false);
                await Context.Message.DeleteAsync(new RequestOptions() { AuditLogReason = "Channelboard security" }).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
