using Discord.Commands;
using Grillbot.Database.Repository;
using Grillbot.Services.Initiable;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class CommandExecutedHandler : IInitiable, IDisposable
    {
        private CommandService CommandService { get; }
        private IServiceProvider Services { get; }
        private ILogger<CommandExecutedHandler> Logger { get; }
        private InternalStatistics InternalStatistics { get; }

        public CommandExecutedHandler(CommandService commandService, IServiceProvider services, ILogger<CommandExecutedHandler> logger,
            InternalStatistics internalStatistics)
        {
            CommandService = commandService;
            Services = services;
            Logger = logger;
            InternalStatistics = internalStatistics;
        }

        private async Task CommandExecutedAsync(Discord.Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified) return;

            var cmd = command.Value;

            var guild = context.Guild == null ? "NoGuild" : $"{context.Guild.Name} ({context.Guild.Id})";
            var channel = context.Channel == null ? "NoChannel" : $"#{context.Channel.Name} ({context.Channel.Id})";
            var args = $"{guild}, {channel}, @{context.User}, ({context.Message.Content})";
            var commandName = $"{cmd.Module.Group} {cmd.Name}".Trim();

            Logger.LogInformation("Executed {0}.\t{1}", commandName, args);
            InternalStatistics.IncrementCommand(commandName);

            using var configRepository = Services.GetService<ConfigRepository>();
            configRepository.IncrementUsageCounter(context.Guild, cmd.Module.Group, cmd.Name);
        }

        public void Dispose()
        {
            CommandService.CommandExecuted -= CommandExecutedAsync;
        }

        public void Init()
        {
            CommandService.CommandExecuted += CommandExecutedAsync;
        }

        public async Task InitAsync() { }
    }
}
