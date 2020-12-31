using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Services.Audit;
using System;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("log")]
    [ModuleID(nameof(AuditLogModule))]
    [Name("Ovládání logování")]
    public class AuditLogModule : BotModuleBase
    {
        public AuditLogModule(IServiceProvider provider) : base(provider: provider)
        {
        }

        [Command("clear")]
        [Summary("**NENÁVRATNĚ** Smaže všechny logy před zadaným datem.")]
        public async Task ClearOldLogsAsync(DateTime before)
        {
            var infoMessage = await ReplyAsync("Probíhá čištění.");

            using (Context.Channel.EnterTypingState())
            {
                using var service = GetService<AuditService>();
                var clearedCount = await service.Service.ClearOldDataAsync(before, Context.Guild);

                await infoMessage.ModifyAsync(o => o.Content = $"Čištění dokončeno.\nVyčištěno záznamů: {clearedCount.FormatWithSpaces()}");
            }
        }
    }
}
