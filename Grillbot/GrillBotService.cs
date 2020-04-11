using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Grillbot.Exceptions;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.Options;
using Grillbot.Services.Statistics;
using Grillbot.Services.Initiable;
using Grillbot.Models.Config.AppSettings;

namespace Grillbot
{
    public class GrillBotService : IHostedService
    {
        public static TimeSpan DatabaseSyncPeriod { get; } = TimeSpan.FromSeconds(60);

        private IServiceProvider Services { get; }
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private Configuration Config { get; }
        private InitService InitService { get; }
        private InternalStatistics InternalStatistics { get; }

        public GrillBotService(IServiceProvider services, DiscordSocketClient client, CommandService commands, IOptions<Configuration> config,
            InternalStatistics internalStatistics, InitService initService)
        {
            Services = services;
            Client = client;
            Commands = commands;
            Config = config.Value;
            InternalStatistics = internalStatistics;
            InitService = initService;

            Client.Ready += OnClientReadyAsync;
        }

        private async Task OnClientReadyAsync()
        {
            InternalStatistics.IncrementEvent("Ready");
            await InitService.InitAsync().ConfigureAwait(false);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(Config.Discord.Token))
                throw new ConfigException("Missing bot token in config.");

            await Client.LoginAsync(TokenType.Bot, Config.Discord.Token).ConfigureAwait(false);
            await Client.StartAsync().ConfigureAwait(false);
            await SetActivity(Config.Discord.Activity).ConfigureAwait(false);

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.StopAsync().ConfigureAwait(false);
            await Client.LogoutAsync().ConfigureAwait(false);
            Client.Dispose();
        }

        private string FormatActivity(string template)
        {
            if (template.Contains("{DateTimeNow:"))
            {
                var templateFields = template.Split(new[] { "{DateTimeNow:" }, StringSplitOptions.RemoveEmptyEntries);
                var otherTemplateFields = templateFields[1].Split("}");
                template = templateFields[0] + DateTime.Now.ToString(otherTemplateFields[0]) + otherTemplateFields[1];
            }

            return template;
        }

        private async Task SetActivity(string activityMessage)
        {
            if (!string.IsNullOrEmpty(activityMessage))
                await Client.SetGameAsync(FormatActivity(activityMessage)).ConfigureAwait(false);
            else
                await Client.SetGameAsync(null).ConfigureAwait(false);
        }
    }
}
