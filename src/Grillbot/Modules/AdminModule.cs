using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Services.AdminServices;
using Grillbot.Services.MessageCache;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [ModuleID(nameof(AdminModule))]
    [Name("Administrační funkce")]
    public class AdminModule : BotModuleBase
    {
        public AdminModule(IServiceProvider provider) : base(provider: provider)
        {
        }

        [Command("pinpurge")]
        [Summary("Hromadné odpinování zpráv.")]
        [Remarks("Poslední parametr skipCount je volitelný. Výchozí hodnota je 0.")]
        public async Task PinPurge(ITextChannel channel, int takeCount, int skipCount = 0)
        {
            var message = await ReplyAsync("Probíhá úklid.");
            var typingState = Context.Channel.EnterTypingState();

            try
            {
                using var service = GetService<PinManagement>();
                var result = await service.Service.PinPurgeAsync(channel, takeCount, skipCount);
                await message.ModifyAsync(m => m.Content = $"Úklid pinů dokončen. Uklizeno pinů: **{result.FormatWithSpaces()}**");
            }
            finally
            {
                typingState.Dispose();
            }
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

            count++; // Include command message.
            var messages = await channel.GetMessagesAsync(count, options: options).FlattenAsync();

            var olderTwoWeeks = messages.Where(o => (DateTime.UtcNow - o.CreatedAt).TotalDays >= 14.0);
            var newerTwoWeeks = messages.Where(o => (DateTime.UtcNow - o.CreatedAt).TotalDays < 14.0);

            await channel.DeleteMessagesAsync(newerTwoWeeks, options);

            foreach (var oldMessage in olderTwoWeeks)
            {
                await oldMessage.DeleteMessageAsync(options);
            }

            using var messageCache = GetService<IMessageCache>();
            messageCache.Service.TryBulkDelete(messages.Select(o => o.Id));
            await ReplyAndDeleteAsync($"Počet smazaných zpráv: **{messages.Count()}**", deleteOptions: options);
        }
    }
}
