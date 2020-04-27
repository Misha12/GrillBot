using Discord;
using Discord.Commands;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grillbot.Helpers;
using Grillbot.Services.Preconditions;
using Discord.WebSocket;
using Grillbot.Exceptions;
using Newtonsoft.Json;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Channelboard;
using Grillbot.Models.Embed;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Name("Channel leaderboards")]
    public class ChannelboardModule : BotModuleBase
    {
        private ChannelStats Stats { get; }
        private ChannelboardWeb ChannelboardWeb { get; }

        public ChannelboardModule(ChannelStats channelStats, ChannelboardWeb channelboardWeb)
        {
            Stats = channelStats;
            ChannelboardWeb = channelboardWeb;
        }

        [Command("channelboard")]
        public async Task ChannelboardAsync()
        {
            var data = await Stats.GetChannelboardDataAsync(Context.Guild, Context.User, ChannelStats.ChannelboardTakeTop);

            if (data.Count == 0)
                await Context.Message.Author.SendPrivateMessageAsync("Ještě nejsou zaznamenány žádné kanály pro tento server.");

            var embed = new BotEmbed(Context.User, null, "Channel leaderboard");

            var messageBuilder = new StringBuilder();
            for (int i = 0; i < data.Count; i++)
            {
                var channelBoardItem = data[i];

                messageBuilder
                    .Append(i + 1).Append(": ").Append(channelBoardItem.ChannelName)
                    .Append(" - ").AppendLine(FormatHelper.FormatWithSpaces(channelBoardItem.Count));
            }

            embed.AddField("=======================", messageBuilder.ToString(), false);
            await Context.Message.Author.SendPrivateMessageAsync(embedBuilder: embed.GetBuilder());
        }

        [Command("channelboardweb")]
        [Summary("Webový leaderboard.")]
        public async Task ChannelboardWebAsync()
        {
            var url = ChannelboardWeb.GetWebUrl(Context);

            var message = $"Tady máš odkaz na channelboard serveru **{Context.Guild.Name}**: {url}.";
            await Context.Message.Author.SendPrivateMessageAsync(message).ConfigureAwait(false);
        }

        [DisabledPM]
        [Command("channelboard", true)]
        [Summary("Počet zpráv v místnosti.")]
        public async Task ChannelboardForRoomAsync()
        {
            if (Context.Message.Tags.Count == 0)
                throw new BotCommandInfoException("Nic jsi netagnul.");

            var channelMention = Context.Message.Tags.FirstOrDefault(o => o.Type == TagType.ChannelMention);
            if (channelMention == null)
                throw new BotCommandInfoException("Netagnul jsi žádný kanál");

            if (!(channelMention.Value is ISocketMessageChannel channel))
                throw new BotException($"Discord.NET uznal, že je to ChannelMention, ale nepovedlo se mi to načíst jako kanál. Prověřte to někdo. (Message: {Context.Message.Content}, Tags: {JsonConvert.SerializeObject(Context.Message.Tags)})");

            var value = await Stats.GetValueAsync(Context.Guild, channel.Id, Context.User);

            if (value == null)
                throw new BotCommandInfoException("Do této místnosti nemáš dostatečná práva.");

            var formatedMessageCount = FormatHelper.FormatWithSpaces(value.Item2);
            var message = $"Aktuální počet zpráv v místnosti **{channel.Name}** je **{formatedMessageCount}** a v příčce se drží na **{value.Item1}**. pozici.";

            await Context.Message.Author.SendPrivateMessageAsync(message);
            await Context.Message.DeleteAsync(new RequestOptions() { AuditLogReason = "Channelboard security" }).ConfigureAwait(false);
        }

        [Command("cleanOldChannels")]
        public async Task CleanOldChannels()
        {
            var clearedChannels = await Stats.CleanOldChannels(Context.Guild);

            await ReplyChunkedAsync(clearedChannels, 10);
            await ReplyAsync("Čištění dokončeno.").ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ChannelboardWeb.Dispose();

            base.Dispose(disposing);
        }
    }
}
