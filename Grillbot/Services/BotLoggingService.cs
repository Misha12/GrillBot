using Discord;
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
        private bool IsDevelopment { get; set; }
        private ulong? ErrorTagUser { get; set; }
        private IConfiguration Config { get; set; }

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

            var isDevelopment = config["IsDevelopment"];
            if (!string.IsNullOrEmpty(isDevelopment)) IsDevelopment = Convert.ToBoolean(isDevelopment);

            var errorTagUser = discordLog["ErrorTagUser"];
            if (!string.IsNullOrEmpty(errorTagUser)) ErrorTagUser = Convert.ToUInt64(errorTagUser);

            Config = config;
        }

        private string GetLogFilename() => Path.Combine(LogDirectory, $"{DateTime.UtcNow.ToString("yyMMdd")}_WatchDog.log");

        private async Task OnLogAsync(LogMessage message)
        {
            var logFilename = GetLogFilename();
            await File.AppendAllTextAsync(logFilename, message.ToString() + Environment.NewLine);

            if (message.Exception != null && LogRoom != null && !IsWebSocketException(message.Exception))
            {
                var exceptionRule = GetExceptionRule(message.Exception);
                var exceptionMessage = message.Exception.ToString();
                var parts = exceptionMessage.SplitInParts(1950).ToArray();
                var channel = Client.GetChannel(LogRoom.Value) as IMessageChannel;

                await SendLogMessageAsync(parts, exceptionRule, channel);
            }

            if (IsDevelopment)
            {
                await Console.Out.WriteLineAsync(message.ToString());
            }
        }

        private async Task SendLogMessageAsync(string[] parts, IConfigurationSection rule, IMessageChannel channel)
        {
            if (rule != null && !string.IsNullOrEmpty(rule["Operation"]) && rule["Operation"] == "Ignore") return;

            if(rule != null && !string.IsNullOrEmpty(rule["Operation"]) && rule["Operation"] == "NoTag")
            {
                for (var i = 0; i < parts.Length; i++)
                {
                    if (i == 0)
                    {
                        await channel?.SendMessageAsync($"```{parts[0]}```");
                    }
                    else
                    {
                        await channel?.SendMessageAsync($"```{parts[i]}```");
                    }
                }
            }
            else
            {
                for (var i = 0; i < parts.Length; i++)
                {
                    if(ErrorTagUser == null)
                    {
                        await channel?.SendMessageAsync($"```{parts[i]}```");
                        continue;
                    }

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
        }

        private IConfigurationSection GetExceptionRule(Exception exception)
        {
            var ruleName = exception.GetType().Name;

            var rules = Config.GetSection("Log:LogToDiscord:ExceptionRules").GetChildren()
                .Where(o => o["Type"] == ruleName).ToList();

            if(exception.InnerException == null)
                return rules.FirstOrDefault();

            return rules.FirstOrDefault(o => o["InnerType"] == exception.InnerException.GetType().Name);
        }

        public void ConfigChanged(IConfiguration newConfig)
        {
            Init(newConfig);
        }

        private bool IsWebSocketException(Exception ex)
        {
            return ex.InnerException != null && (ex.InnerException is WebSocketException || ex.InnerException is WebSocketClosedException);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client.Log -= OnLogAsync;
                Commands.Log -= OnLogAsync;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
