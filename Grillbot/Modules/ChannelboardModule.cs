using Discord;
using Discord.Commands;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grillbot.Helpers;
using Grillbot.Services.Statistics;
using Grillbot.Services.Preconditions;
using Discord.WebSocket;
using Grillbot.Exceptions;

namespace Grillbot.Modules
{
    [Name("Channel leaderboards")]
    [RequirePermissions("Channelboard", DisabledForPM = true, BoosterAllowed = true)]
    public class ChannelboardModule : BotModuleBase
    {
        private ChannelStats Stats { get; }

        public ChannelboardModule(Statistics statistics)
        {
            Stats = statistics.ChannelStats;
        }

        [Command("channelboard")]
        public async Task ChannelboardAsync()
        {
            await ChannelboardAsync(ChannelStats.ChannelboardTakeTop);
        }

        [Command("channelboard")]
        [Remarks("Možnost zvolit TOP N kanálů.")]
        public async Task ChannelboardAsync(int takeTop)
        {
            var channelBoardData = Stats.Counter
                .OrderByDescending(o => o.Value)
                .Where(o => CanAuthorToChannel(o.Key))
                .Take(takeTop).ToList();

            var messageBuilder = new StringBuilder()
                .AppendLine("=======================")
                .AppendLine("|\tCHANNEL LEADERBOARD\t|")
                .AppendLine("=======================");

            for (int i = 0; i < channelBoardData.Count; i++)
            {
                var channelBoardItem = channelBoardData[i];
                var channel = Context.Client.GetChannel(channelBoardItem.Key) as IMessageChannel;

                messageBuilder
                    .Append(i + 1).Append(": ").Append(channel.Name)
                    .Append(" - ").Append(FormatHelper.FormatWithSpaces(channelBoardItem.Value))
                    .AppendLine();
            }

            await Context.Message.Author.SendMessageAsync(messageBuilder.ToString());
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
            await Context.Message.Author.SendMessageAsync(message);
        }

        [Command("channelboard")]
        [Summary("Počet zpráv v místnosti.")]
        public async Task ChannelboardForRoomAsync(string roomMention)
        {
            if (Context.Message.Tags.Count == 0)
            {
                await ReplyAsync("Netagnul jsi žádný kanál.");
                return;
            }

            if (!(Context.Message.Tags.FirstOrDefault(o => o.Type == TagType.ChannelMention).Value is ISocketMessageChannel channel))
                throw new BotException("Discord.NET uznal, že je to ChannelMention, ale nepovedlo se mi to načíst jako kanál. Prověřte to někdo pls.");

            if (!CanAuthorToChannel(channel.Id))
                await Context.Message.Author.SendMessageAsync("Do této místnosti nemáš dostatečná práva.");

            var value = Stats.GetValue(channel.Id);
            var formatedMessageCount = FormatHelper.FormatWithSpaces(value.Item2);
            var message = $"Aktuální počet zpráv v místnosti **{channel.Name}** je **{formatedMessageCount}** a v příčce se drží na **{value.Item1}**. pozici.";

            await Context.Message.Author.SendMessageAsync(message);
            await Context.Message.DeleteAsync(new RequestOptions() { AuditLogReason = "Channelboard security" });
        }
    }
}
