using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;
using GrilBot.Exceptions;

namespace GrilBot
{
    public class StartupService : IConfigChangeable
    {
        private IServiceProvider Services { get; }
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IConfigurationRoot Config { get; set; }

        public StartupService(IServiceProvider services, DiscordSocketClient client, CommandService commands, IConfigurationRoot config)
        {
            Services = services;
            Client = client;
            Commands = commands;
            Config = config;
        }

        public async Task StartAsync()
        {
            var dcConfig = Config.GetSection("Discord");
            var token = dcConfig["Token"];
            var activityMessage = dcConfig["Activity"];

            if (string.IsNullOrEmpty(token))
                throw new ConfigException("Missing bot token in config.");

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            await SetActivity(activityMessage);

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
        }

        private string FormatActivity(string template)
        {
            if(template.Contains("{DateTimeNow:"))
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

        public void ConfigChanged(IConfigurationRoot newConfig)
        {
            Config = newConfig;
            SetActivity(newConfig["Discord:Activity"]).Wait();
        }
    }
}
