using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Grillbot.Exceptions;
using Grillbot.Services.Config;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.Options;
using Grillbot.Services.Config.Models;
using Grillbot.Services;

namespace Grillbot
{
    public class GrillBotService : IConfigChangeable, IHostedService
    {
        public static TimeSpan DatabaseSyncPeriod { get; } = TimeSpan.FromSeconds(60);

        private IServiceProvider Services { get; }
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private Configuration Config { get; set; }
        private IMessageCache Cache { get; set; }
        private TempUnverifyService TempUnverify { get; }

        public GrillBotService(IServiceProvider services, DiscordSocketClient client, CommandService commands, IMessageCache cache,
            IOptions<Configuration> config, TempUnverifyService tempUnverify)
        {
            Services = services;
            Client = client;
            Commands = commands;
            Config = config.Value;
            Cache = cache;
            TempUnverify = tempUnverify;

            Client.Ready += OnClientReadyAsync;
        }

        private async Task OnClientReadyAsync()
        {
            await TempUnverify.InitAsync();
            await Cache.InitAsync();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(Config.Discord.Token))
                throw new ConfigException("Missing bot token in config.");

            await Client.LoginAsync(TokenType.Bot, Config.Discord.Token);
            await Client.StartAsync();
            await SetActivity(Config.Discord.Activity);

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.StopAsync();
            await Client.LogoutAsync();
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
                await Client.SetGameAsync(FormatActivity(activityMessage));
            else
                await Client.SetGameAsync(null);
        }

        public void ConfigChanged(Configuration newConfig)
        {
            Config = newConfig;
            SetActivity(newConfig.Discord.Activity).Wait();
        }
    }
}
