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
using Microsoft.Extensions.DependencyInjection;
using Grillbot.Database.Repository;
using Grillbot.Enums;
using System.Linq;
using Grillbot.Modules;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

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
        private ILogger<GrillBotService> Logger { get; }

        public GrillBotService(IServiceProvider services, DiscordSocketClient client, CommandService commands, IOptions<Configuration> config,
            InternalStatistics internalStatistics, InitService initService, ILogger<GrillBotService> logger)
        {
            Services = services;
            Client = client;
            Commands = commands;
            Config = config.Value;
            InternalStatistics = internalStatistics;
            InitService = initService;
            Logger = logger;

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

            await Client.LoginAsync(TokenType.Bot, Config.Discord.Token);
            await Client.StartAsync();
            await SetActivity(Config.Discord.Activity);

            Commands.AddTypeReader<JObject>(new JObjectTypeReader());

            await AddModulesAsync();
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

        private async Task AddModulesAsync()
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetRequiredService<GlobalConfigRepository>();

            var unloadedModules = new List<string>();
            var unloadedModulesConfig = await repository.GetItemAsync(GlobalConfigItems.UnloadedModules);

            if (!string.IsNullOrEmpty(unloadedModulesConfig))
                unloadedModules.AddRange(JsonConvert.DeserializeObject<List<string>>(unloadedModulesConfig));

            var moduleBase = typeof(BotModuleBase);
            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(o => !o.IsAbstract && moduleBase.IsAssignableFrom(o) && !unloadedModules.Contains(o.Name));

            foreach (var moduleType in types)
            {
                await Commands.AddModuleAsync(moduleType, Services);
                Logger.LogInformation($"Initialized module {moduleType.FullName}");
            }
        }
    }
}
