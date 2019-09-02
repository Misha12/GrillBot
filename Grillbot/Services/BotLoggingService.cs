﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Extensions;
using System.Net.WebSockets;
using Discord.Net;
using Grillbot.Services.Config;

namespace Grillbot.Services
{
    public class BotLoggingService : IConfigChangeable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }

        private string LogDirectory { get; set; }
        private ulong? LogRoom { get; set; }
        private ulong? ErrorTagUser { get; set; }

        public BotLoggingService(DiscordSocketClient client, CommandService commands, IConfiguration config)
        {
            Client = client;
            Commands = commands;
            Init(config);
            Client.Log += OnLogAsync;
            Commands.Log += OnLogAsync;
        }

        private void Init(IConfiguration config)
        {
            var logDir = config["Log:Path"].ToString();
            if (string.IsNullOrEmpty(logDir)) logDir = Environment.CurrentDirectory;
            LogDirectory = Path.Combine(logDir, "logs");

            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);

            var discordLog = config.GetSection("Log:LogToDiscord");
            if (Convert.ToBoolean(discordLog["Enabled"]))
            {
                LogRoom = Convert.ToUInt64(discordLog["Room"]);
            }

            var errorTagUser = discordLog["ErrorTagUser"];
            if (!string.IsNullOrEmpty(errorTagUser)) ErrorTagUser = Convert.ToUInt64(errorTagUser);
        }

        private async Task OnLogAsync(LogMessage message)
        {
            await PostException(message);
            await Console.Out.WriteLineAsync(message.ToString());
        }

        private async Task SendLogMessageAsync(string[] parts, IMessageChannel channel)
        {
            for (var i = 0; i < parts.Length; i++)
            {
                if (ErrorTagUser == null)
                {
                    await channel?.SendMessageAsync($"```{parts[i]}```");
                    continue;
                }

                if (i == 0)
                    await channel?.SendMessageAsync($"<@{ErrorTagUser}> ```{parts[0]}```");
                else
                    await channel?.SendMessageAsync($"```{parts[i]}```");
            }
        }

        private async Task PostException(LogMessage message)
        {
            if (!CanSendToDiscord(message)) return;

            var exceptionMessage = message.Exception.ToString();
            var parts = exceptionMessage.SplitInParts(1950).ToArray();

            if (Client.GetChannel(LogRoom.Value) is IMessageChannel channel)
            {
                await SendLogMessageAsync(parts, channel);
            }
        }

        public void ConfigChanged(IConfiguration newConfig)
        {
            Init(newConfig);
        }

        private bool IsWebSocketException(Exception ex)
        {
            return ex.InnerException != null && (ex.InnerException is WebSocketException || ex.InnerException is WebSocketClosedException);
        }

        private bool CanSendToDiscord(LogMessage message)
        {
            var haveException = message.Exception != null;
            var haveLogRoom = LogRoom != null;
            var isWebSocketException = haveException && IsWebSocketException(message.Exception);

            return haveException && haveLogRoom && !isWebSocketException;
        }

        public void Dispose()
        {
            Client.Log -= OnLogAsync;
            Commands.Log -= OnLogAsync;
        }
    }
}
