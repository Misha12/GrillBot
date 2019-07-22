using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WatchDog_Bot.Extensions;

namespace WatchDog_Bot
{
    public class LoggingService : IConfigChangeable
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }

        private string LogDirectory { get; set; }
        private ulong? LogRoom { get; set; }
        private bool IsDevelopment { get; set; }
        private ulong? ErrorTagUser { get; set; }

        public LoggingService(DiscordSocketClient client, CommandService commands, IConfigurationRoot config)
        {
            Client = client;
            Commands = commands;
            Init(config);
            Client.Log += OnLogAsync;
            Commands.Log += OnLogAsync;
        }

        private void Init(IConfigurationRoot config)
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

            var isDevelopment = config["IsDevelopment"];
            if (!string.IsNullOrEmpty(isDevelopment)) IsDevelopment = Convert.ToBoolean(isDevelopment);

            var errorTagUser = discordLog["ErrorTagUser"];
            if (!string.IsNullOrEmpty(errorTagUser)) ErrorTagUser = Convert.ToUInt64(errorTagUser);
        }

        private string GetLogFilename() => Path.Combine(LogDirectory, $"{DateTime.UtcNow.ToString("yyMMdd")}_WatchDog.log");

        private async Task OnLogAsync(LogMessage message)
        {
            var logFilename = GetLogFilename();
            await File.AppendAllTextAsync(logFilename, message.ToString() + Environment.NewLine);

            if (message.Exception != null && LogRoom != null)
            {
                var exceptionMessage = message.Exception.ToString();
                var parts = exceptionMessage.SplitInParts(1950).ToArray();
                var channel = Client.GetChannel(LogRoom.Value) as IMessageChannel;

                for(var i = 0; i < parts.Length; i++)
                {
                    if (i == 0)
                    {
                        await channel?.SendMessageAsync($"<@{ErrorTagUser}> ```{parts[0]}```");
                    }
                    else
                    {
                        await channel?.SendMessageAsync($"```{parts[i]}```");
                    }
                }
            }

            if (IsDevelopment)
            {
                await Console.Out.WriteLineAsync(message.ToString());
            }
        }

        public void ConfigChanged(IConfigurationRoot newConfig)
        {
            Init(newConfig);
        }
    }
}
