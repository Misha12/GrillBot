using Discord.Commands;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Services;
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
        private BotStatusService BotStatus { get; }

        public CommandExecutedHandler(CommandService commandService, IServiceProvider services, ILogger<CommandExecutedHandler> logger,
            InternalStatistics internalStatistics, BotStatusService botStatus)
        {
            CommandService = commandService;
            Services = services;
            Logger = logger;
            InternalStatistics = internalStatistics;
            BotStatus = botStatus;
        }

        private async Task CommandExecutedAsync(Discord.Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if(!result.IsSuccess && result.Error != null)
            {
                switch(result.Error.Value)
                {
                    case CommandError.UnmetPrecondition:
                    case CommandError.ParseFailed:
                    case CommandError.Unsuccessful:
                        await context.Channel.SendMessageAsync(result.ErrorReason.PreventMassTags());
                        break;
                    case CommandError.BadArgCount:
                        await SendCommandHelp(context, 1);
                        break;
                }
            }

            LogCommand(command, context);
            BotStatus.RunningCommands.RemoveAll(o => o.Id == context.Message.Id);
        }

        private void LogCommand(Discord.Optional<CommandInfo> command, ICommandContext context)
        {
            var cmd = command.Value;

            var guild = context.Guild == null ? "NoGuild" : $"{context.Guild.Name} ({context.Guild.Id})";
            var channel = context.Channel == null ? "NoChannel" : $"#{context.Channel.Name} ({context.Channel.Id})";
            var args = $"{guild}, {channel}, @{context.User}, ({context.Message.Content})";
            var commandName = $"{cmd.Module.Group} {cmd.Name}".Trim();

            Logger.LogInformation("Executed {0}.\t{1}", commandName, args);
            InternalStatistics.IncrementCommand(commandName);

            if(context.Guild != null)
            {
                using var scope = Services.CreateScope();
                using var configRepository = scope.ServiceProvider.GetService<ConfigRepository>();
                configRepository.IncrementUsageCounter(context.Guild, cmd.Module.Group, cmd.Name);
            }
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

        private async Task SendCommandHelp(ICommandContext context, int argPos)
        {
            var helpCommand = $"grillhelp {context.Message.Content.Substring(argPos)}";
            await CommandService.ExecuteAsync(context, helpCommand, Services).ConfigureAwait(false);
        }
    }
}
