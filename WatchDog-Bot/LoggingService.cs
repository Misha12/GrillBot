using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WatchDog_Bot
{
    public class LoggingService
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }

        private string LogDirectory { get; }
        private ulong? LogRoom { get; }
        private bool IsDevelopment { get; }

        public LoggingService(DiscordSocketClient client, CommandService commands, IConfigurationRoot config)
        {
            Client = client;
            Commands = commands;

            var logDir = config["Log:Path"].ToString();
            if (string.IsNullOrEmpty(logDir)) logDir = Environment.CurrentDirectory;
            LogDirectory = Path.Combine(logDir, "logs");

            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);

            var discordLog = config.GetSection("Log:LogToDiscord");
            if(Convert.ToBoolean(discordLog["Enabled"]))
            {
                LogRoom = Convert.ToUInt64(discordLog["Room"]);
            }

            var isDevelopment = config["IsDevelopment"];
            if (!string.IsNullOrEmpty(isDevelopment)) IsDevelopment = Convert.ToBoolean(isDevelopment);

            Client.Log += OnLogAsync;
            Commands.Log += OnLogAsync;
        }

        private string GetLogFilename() => Path.Combine(LogDirectory, $"{DateTime.UtcNow.ToString("yyMMdd")}_WatchDog.log");

        private async Task OnLogAsync(LogMessage message)
        {
            var logFilename = GetLogFilename();
            await File.AppendAllTextAsync(logFilename, message.ToString() + Environment.NewLine);

            if(message.Exception != null && LogRoom != null)
            {
                var channel = Client.GetChannel(LogRoom.Value) as IMessageChannel;
                await channel?.SendMessageAsync(message.Exception.ToString());
            }

            if(IsDevelopment)
            {
                await Console.Out.WriteLineAsync(message.ToString());
            }
        }
    }
}
