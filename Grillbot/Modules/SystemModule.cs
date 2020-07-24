using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Services;
using Grillbot.Services.TempUnverify;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("system")]
    [Name("Interní správa bota")]
    [ModuleID("SystemModule")]
    public class SystemModule : BotModuleBase
    {
        private ILogger<SystemModule> Logger { get; }
        private IHostApplicationLifetime Lifetime { get; }
        private BotStatusService BotStatus { get; }
        private TempUnverifyService TempUnverify { get; }

        public SystemModule(ILogger<SystemModule> logger, IHostApplicationLifetime lifetime, BotStatusService botStatus,
            TempUnverifyService tempUnverify)
        {
            Logger = logger;
            Lifetime = lifetime;
            BotStatus = botStatus;
            TempUnverify = tempUnverify;
        }

        [Command("send")]
        [Summary("Odeslání zprávy do kanálu.")]
        public async Task SendAsync(ITextChannel textChannel, [Remainder] string message)
        {
            Logger.LogInformation($"{Context.User.GetFullName()} send message to {textChannel.Name}. Content: {message}");
            await textChannel.SendMessageAsync(message);
        }

        [Command("shutdown_force")]
        [Summary("Násilné ukončení aplikace.")]
        public async Task ShutdownForceAsync()
        {
            await ReplyAsync("Probíhá násilné ukončování.");
            Lifetime.StopApplication();
        }

        [Command("shutdown")]
        [Summary("Ukončení aplikace")]
        public async Task ShutdownAsync()
        {
            var message = await ReplyAsync("Probíhá příprava ukončení.");
            var cannotShutdownData = new List<string>();

            var workingCommands = BotStatus.RunningCommands.Take(BotStatus.RunningCommands.Count - 1);
            if (workingCommands.Any())
            {
                var runningCommands = workingCommands.Select(o =>
                {
                    var prefix = $"> `{o.Author.GetFullName()}` (#{o.Channel.Name}) ({o.CreatedAt.LocalDateTime.ToLocaleDatetime()}):";
                    return $"{prefix} `{o.Content.Cut(100)}`";
                });

                cannotShutdownData.Add("**Běžící příkazy**:");
                cannotShutdownData.AddRange(runningCommands);
            }

            if (TempUnverify.ReturningAcccessFor.Count > 0)
            {
                var users = TempUnverify.ReturningAcccessFor.Select(o => $"> `{o.GetFullName()}` ({o.Id})");

                cannotShutdownData.Add("**Vracení přístupu pro uživatele**:");
                cannotShutdownData.AddRange(users);
            }

            if (cannotShutdownData.Count > 0)
            {
                await message.ModifyAsync(o => o.Content = "Nyní nelze provést vypnutí, protože probíhají následující operace:");
                await ReplyChunkedAsync(cannotShutdownData, 3);
                return;
            }

            await message.ModifyAsync(o => o.Content = "Probíhá vypínání.");
            Lifetime.StopApplication();
        }
    }
}
