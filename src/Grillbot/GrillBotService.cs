using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Grillbot.Exceptions;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Grillbot.Services.Statistics;
using Grillbot.Services.Initiable;
using Newtonsoft.Json.Linq;
using Grillbot.TypeReaders;
using Grillbot.Models;
using System.Text;
using Grillbot.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Grillbot
{
    public class GrillBotService : IHostedService
    {
        private IServiceProvider Services { get; }
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private InitService InitService { get; }
        private InternalStatistics InternalStatistics { get; }
        private BotState BotState { get; }
        private IConfiguration Configuration { get; }

        public GrillBotService(IServiceProvider services, DiscordSocketClient client, CommandService commands, InternalStatistics internalStatistics,
            InitService initService, BotState botState, IConfiguration configuration)
        {
            Services = services;
            Client = client;
            Commands = commands;
            InternalStatistics = internalStatistics;
            InitService = initService;
            BotState = botState;
            Configuration = configuration;

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
            if (string.IsNullOrEmpty(Configuration["Token"]))
                throw new ConfigException("Missing bot token in config.");

            await Client.LoginAsync(TokenType.Bot, Configuration["Token"]);
            await Client.StartAsync();

            BotState.AppInfo = await Client.GetApplicationInfoAsync();

            Commands.AddTypeReader<JObject>(new JObjectTypeReader());
            Commands.AddTypeReader<GroupCommandMatch>(new GroupCommandMatchTypeReader());
            Commands.AddTypeReader<EmoteInfoOrderType>(new EnumTypeReader<EmoteInfoOrderType>(false));
            Commands.AddTypeReader<SortType>(new EnumTypeReader<SortType>(false));

            using var scope = Services.CreateScope();
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), scope.ServiceProvider);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.StopAsync().ConfigureAwait(false);
            await Client.LogoutAsync().ConfigureAwait(false);
            Client.Dispose();
        }

        private Task SetActivityAsync()
        {
            var activityMessage = Configuration["ActivityMessage"];

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
