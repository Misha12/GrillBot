﻿using Discord;
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

namespace Grillbot
{
    public class GrillBotService : IConfigChangeable, IHostedService
    {
        public const int MaxEmbedFields = 20;

        private IServiceProvider Services { get; }
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IConfiguration Config { get; set; }
        private IMessageCache Cache { get; set; }

        public GrillBotService(IServiceProvider services, DiscordSocketClient client, CommandService commands, IConfiguration config,
            IMessageCache cache)
        {
            Services = services;
            Client = client;
            Commands = commands;
            Config = config;
            Cache = cache;

            Client.Ready += OnClientReadyAsync;
        }

        private async Task OnClientReadyAsync()
        {
            await Cache.InitAsync();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var config = Config.GetSection("Discord");
            var token = config["Token"];

            if (string.IsNullOrEmpty(token))
                throw new ConfigException("Missing bot token in config.");

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            await SetActivity(config["Activity"]);

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

        public void ConfigChanged(IConfiguration newConfig)
        {
            Config = newConfig;
            SetActivity(newConfig["Discord:Activity"]).Wait();
        }
    }
}
