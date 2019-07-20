using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;
using WatchDog_Bot.Exceptions;

namespace WatchDog_Bot
{
    public class StartupService
    {
        private IServiceProvider Services { get; }
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IConfigurationRoot Config { get; }

        public StartupService(IServiceProvider services, DiscordSocketClient client, CommandService commands, IConfigurationRoot config)
        {
            Services = services;
            Client = client;
            Commands = commands;
            Config = config;
        }

        public async Task StartAsync()
        {
            var token = Config["Discord:Token"];
            if (string.IsNullOrEmpty(token))
                throw new ConfigException("Missing bot token in config.");

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
        }
    }
}
