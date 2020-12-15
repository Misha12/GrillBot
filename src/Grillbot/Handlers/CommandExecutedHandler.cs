using Discord;
using Discord.Commands;
using Grillbot.Database;
using Grillbot.Database.Entity.MethodConfig;
using Grillbot.Extensions;
using Grillbot.Services.Audit;
using Grillbot.Services.Initiable;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Handlers
{
    public class CommandExecutedHandler : IInitiable, IDisposable
    {
        private CommandService CommandService { get; }
        private IServiceProvider Services { get; }
        private ILogger<CommandExecutedHandler> Logger { get; }
        private BotState BotState { get; }
        private InternalStatistics InternalStatistics { get; }

        public CommandExecutedHandler(CommandService commandService, IServiceProvider services, ILogger<CommandExecutedHandler> logger, BotState botState,
            InternalStatistics internalStatistics)
        {
            CommandService = commandService;
            Services = services;
            Logger = logger;
            BotState = botState;
            InternalStatistics = internalStatistics;
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess && result.Error != null)
            {
                switch (result.Error.Value)
                {
                    case CommandError.UnmetPrecondition:
                    case CommandError.ParseFailed:
                    case CommandError.Unsuccessful:
                        await context.Channel.SendMessageAsync(result.ErrorReason.PreventMassTags());
                        break;
                    case CommandError.BadArgCount:
                        await SendCommandHelp(context, 1);
                        break;
                    case CommandError.UnknownCommand:
                        await ProcessUnknownCommandAsync(context);
                        break;
                    case CommandError.ObjectNotFound when result is ParseResult parseResult && typeof(IUser).IsAssignableFrom(parseResult.ErrorParameter.Type):
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Zadaný uživatel nebyl nalezen.");
                        break;
                }
            }

            using var scope = Services.CreateScope();

            try
            {
                var auditService = scope.ServiceProvider.GetService<AuditService>();
                await auditService.LogCommandAsync(command, context);

                await LogCommandAsync(command, context, scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
            }

            BotState.RunningCommands.RemoveAll(o => o.Id == context.Message.Id);
        }

        private async Task LogCommandAsync(Optional<CommandInfo> command, ICommandContext context, IServiceProvider scopedProvider)
        {
            var guild = context.Guild == null ? "NoGuild" : $"{context.Guild.Name} ({context.Guild.Id})";
            var channel = context.Channel == null ? "NoChannel" : $"#{context.Channel.Name} ({context.Channel.Id})";
            var args = $"{guild}, {channel}, @{context.User}, ({context.Message.Content})";
            var commandName = command.IsSpecified ? $"{command.Value.Module.Group} {command.Value.Name}".Trim() : "Unknown command";

            Logger.LogInformation("Executed {0}.\t{1}", commandName, args);

            if (command.IsSpecified)
                InternalStatistics.IncrementCommand(commandName);

            if (context.Guild != null && command.IsSpecified)
            {
                var cmd = command.Value;

                var grillBotRepository = scopedProvider.GetService<IGrillBotRepository>();

                var config = await grillBotRepository.ConfigRepository.FindConfigAsync(context.Guild.Id, cmd.Module.Group, cmd.Name, false);
                if (config == null)
                {
                    config = MethodsConfig.Create(context.Guild, cmd.Module.Group, cmd.Name, false, new JObject());
                    grillBotRepository.Add(config);
                }

                config.UsedCount++;
                await grillBotRepository.CommitAsync();
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

#pragma warning disable S1172 // Unused method parameters should be removed
        private async Task SendCommandHelp(ICommandContext context, int argPos)
#pragma warning restore S1172 // Unused method parameters should be removed
        {
            var helpCommand = $"grillhelp {context.Message.Content[argPos..]}";
            await CommandService.ExecuteAsync(context, helpCommand, Services).ConfigureAwait(false);
        }

        private async Task ProcessUnknownCommandAsync(ICommandContext context)
        {
            var group = context.Message.Content[1..];
            var module = CommandService.Modules.FirstOrDefault(o => o.Group == group || o.Aliases.Contains(group));

            if (module != null)
                await SendCommandHelp(context, 1);
        }
    }
}
