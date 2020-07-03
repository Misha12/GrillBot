using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Services.AdminServices;
using Grillbot.Services.MessageCache;
using Grillbot.Services.Permissions.Preconditions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [ModuleID("AdminModule")]
    [Name("Administrační funkce")]
    public class AdminModule : BotModuleBase
    {
        private IMessageCache MessageCache { get; }
        private PinManagement PinManagement { get; }

        public AdminModule(ConfigRepository config, IMessageCache messageCache, PinManagement pinManagement) : base(configRepository: config)
        {
            MessageCache = messageCache;
            PinManagement = pinManagement;
        }

        [Command("pinpurge")]
        [Summary("Hromadné odpinování zpráv.")]
        [Remarks("Poslední parametr skipCount je volitelný. Výchozí hodnota je 0.")]
        public async Task PinPurge(string channel, int takeCount, int skipCount = 0)
        {
            var mentionedChannel = Context.Message.MentionedChannels
                .OfType<SocketTextChannel>()
                .FirstOrDefault(o => $"<#{o.Id}>" == channel);

            if (mentionedChannel != null)
            {
                await PinManagement.PinPurgeAsync(mentionedChannel, takeCount, skipCount);
                return;
            }

            await ReplyAsync($"Odkazovaný textový kanál **{channel}** nebyl nalezen.");
        }

        [Command("clear")]
        [Summary("Hromadné mazání zpráv.")]
        public async Task ClearMessagesAsync(int count)
        {
            var channel = Context.Message.Channel as ITextChannel;
            var options = new RequestOptions()
            {
                AuditLogReason = "Clear command",
                RetryMode = RetryMode.AlwaysRetry,
                Timeout = 30000
            };

            var messages = await channel.GetMessagesAsync(count, options: options).FlattenAsync();

            var olderTwoWeeks = messages.Where(o => (DateTime.UtcNow - o.CreatedAt).TotalDays >= 14.0);
            var newerTwoWeeks = messages.Where(o => (DateTime.UtcNow - o.CreatedAt).TotalDays < 14.0);

            await channel.DeleteMessagesAsync(newerTwoWeeks, options);

            foreach (var oldMessage in olderTwoWeeks)
            {
                await oldMessage.DeleteMessageAsync(options);
            }

            MessageCache.TryBulkDelete(messages.Select(o => o.Id));
            await ReplyAndDeleteAsync($"Počet smazaných zpráv: {messages.Count()}", deleteOptions: options);
        }
    }
}
