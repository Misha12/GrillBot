using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions.Discord;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
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
        private BotState BotState { get; }

        public SystemModule(ILogger<SystemModule> logger, IHostApplicationLifetime lifetime, BotState botState, IServiceProvider provider) : base(provider: provider)
        {
            Logger = logger;
            Lifetime = lifetime;
            BotState = botState;
        }

        [Command("send")]
        [Summary("Odeslání zprávy do kanálu.")]
        public async Task SendAsync(ITextChannel textChannel, [Remainder] string message)
        {
            Logger.LogInformation($"{Context.User.GetFullName()} send message to {textChannel.Name}. Content: {message}");
            await textChannel.SendMessageAsync(message);
        }
    }
}
