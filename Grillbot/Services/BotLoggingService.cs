using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Extensions;
using System.Net.WebSockets;
using Discord.Net;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using System.IO;
using Grillbot.Helpers;

namespace Grillbot.Services
{
    public class BotLoggingService : IConfigChangeable, IDisposable
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }

        private ulong? LogRoom { get; set; }

        public BotLoggingService(DiscordSocketClient client, CommandService commands, IOptions<Configuration> config)
        {
            Client = client;
            Commands = commands;
            Init(config.Value);
            Client.Log += OnLogAsync;
            Commands.Log += OnLogAsync;
        }

        private void Init(Configuration config)
        {
            var logRoom = config.Log.LogRoomID;

            if(!string.IsNullOrEmpty(logRoom))
            {
                LogRoom = Convert.ToUInt64(logRoom);
            }
        }

        private async Task OnLogAsync(LogMessage message)
        {
            await PostException(message);
            await WriteToLogAsync($"{message.Severity}\t{message.Message}", message.Source);
        }

        private async Task SendLogMessageAsync(string[] parts, IMessageChannel channel)
        {
            for (var i = 0; i < parts.Length; i++)
            {
                await channel?.SendMessageAsync($"```{parts[i]}```");
            }
        }

        private async Task PostException(LogMessage message)
        {
            if (!CanSendToDiscord(message)) return;

            var exceptionMessage = message.Exception.ToString();
            var parts = exceptionMessage.SplitInParts(1980).ToArray();

            if (Client.GetChannel(LogRoom.Value) is IMessageChannel channel)
            {
                await SendLogMessageAsync(parts, channel);
            }
        }

        public void ConfigChanged(Configuration newConfig)
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

        public void WriteToLog(string message, string source = "BOT")
        {
            WriteToLogAsync(message, source).GetAwaiter().GetResult();
        }

        public async Task WriteToLogAsync(string message, string source = "BOT")
        {
            await Console.Out.WriteLineAsync($"{DateTime.Now} {source}\t{message}");
        }

        public void SendConfigChangeInfo(string oldHash, string newHash)
        {
            var fileInfo = new FileInfo("appsettings.json");

            var embed = new EmbedBuilder()
            {
                Color = Color.Gold,
                Title = "Konfigurační soubor byl aktualizován"
            };

            embed
                .AddField(o => o.WithIsInline(true).WithName("Starý hash").WithValue(oldHash))
                .AddField(o => o.WithIsInline(true).WithName("Nový hash").WithValue(newHash))
                .AddField(o => o.WithIsInline(true).WithName("Velikost").WithValue(FormatHelper.FormatAsSize(fileInfo.Length)))
                .WithAuthor(Client.CurrentUser)
                .WithCurrentTimestamp();

            if (Client.GetChannel(LogRoom.Value) is IMessageChannel channel)
            {
                channel.SendMessageAsync(embed: embed.Build()).GetAwaiter().GetResult();
            }
        }
    }
}
