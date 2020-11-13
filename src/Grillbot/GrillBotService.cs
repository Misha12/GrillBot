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
using Newtonsoft.Json.Linq;
using Grillbot.TypeReaders;
using Grillbot.Models;
using System.Text;
using Grillbot.Core.Math.Models;
using Grillbot.Enums;
using Grillbot.Services.Config;

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
        private BotState BotState { get; }
        private ConfigurationService ConfigurationService { get; }

        public GrillBotService(IServiceProvider services, DiscordSocketClient client, CommandService commands, IOptions<Configuration> config,
            InternalStatistics internalStatistics, InitService initService, BotState botState, ConfigurationService configurationService)
        {
            Services = services;
            Client = client;
            Commands = commands;
            Config = config.Value;
            InternalStatistics = internalStatistics;
            InitService = initService;
            BotState = botState;
            ConfigurationService = configurationService;

            Client.Ready += OnClientReadyAsync;
        }

        private async Task OnClientReadyAsync()
        {
            InternalStatistics.IncrementEvent("Ready");
            
            await InitService.InitAsync();
            await SetActivityAsync();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(Config.Discord.Token))
                throw new ConfigException("Missing bot token in config.");

            await Client.LoginAsync(TokenType.Bot, Config.Discord.Token);
            await Client.StartAsync();

            BotState.AppInfo = await Client.GetApplicationInfoAsync();

            Commands.AddTypeReader<JObject>(new JObjectTypeReader());
            Commands.AddTypeReader<GroupCommandMatch>(new GroupCommandMatchTypeReader());
            Commands.AddTypeReader<MathSession>(new MathSessionTypeReader());
            Commands.AddTypeReader<GlobalConfigItems>(new GlobalConfigItemTypeReader());

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.StopAsync().ConfigureAwait(false);
            await Client.LogoutAsync().ConfigureAwait(false);
            Client.Dispose();
        }

        private Task SetActivityAsync()
        {
            var activityMessage = ConfigurationService.GetValue(GlobalConfigItems.ActivityMessage);

            if (!string.IsNullOrEmpty(activityMessage) && activityMessage == "None")
                return Client.SetGameAsync(null);

            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(activityMessage))
                builder.Append(activityMessage).Append(" | ");

            builder
                .Append("Running on ")
                .Append(ThisAssembly.Git.Commit).Append('@').Append(ThisAssembly.Git.Branch).Append(". ")
                .Append("Latest tag is ").Append(ThisAssembly.Git.BaseTag).Append('.');

            return Client.SetGameAsync(builder.ToString());
        }
    }
}
