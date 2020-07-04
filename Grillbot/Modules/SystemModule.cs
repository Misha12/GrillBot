using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Permissions.Preconditions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Group("system")]
    [Name("Interní správa bota")]
    [ModuleID("SystemModule")]
    public class SystemModule : BotModuleBase
    {
        private ILogger<SystemModule> Logger { get; }
        private IHostApplicationLifetime Lifetime { get; }

        public SystemModule(ILogger<SystemModule> logger, IHostApplicationLifetime lifetime)
        {
            Logger = logger;
            Lifetime = lifetime;
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
    }
}
