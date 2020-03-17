using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Extensions;
using System.Net.WebSockets;
using Discord.Net;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using Grillbot.Exceptions;
using Microsoft.Extensions.Logging;

namespace Grillbot.Services
{
    public class BotLoggingService : IDisposable
    {
        public const int MessageSizeForException = 1980;

        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IServiceProvider Services { get; }
        private ILogger<BotLoggingService> Logger { get; }

        private ulong? LogRoom { get; set; }

        public BotLoggingService(DiscordSocketClient client, CommandService commands, IOptions<Configuration> config, IServiceProvider services, ILogger<BotLoggingService> logger)
        {
            Client = client;
            Commands = commands;
            Logger = logger;
            Init(config.Value);
            Services = services;
            Client.Log += OnLogAsync;
            Commands.Log += OnLogAsync;
        }

        private void Init(Configuration config)
        {
            var logRoom = config.Log.LogRoomID;

            if (!string.IsNullOrEmpty(logRoom))
            {
                LogRoom = Convert.ToUInt64(logRoom);
            }
        }

        private async Task OnLogAsync(LogMessage message)
        {
            await PostException(message).ConfigureAwait(false);
            Write(message.Severity, message.Message, message.Source, message.Exception);
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
            if (!IsValidException(message)) return;

            if (message.Exception is CommandException ce)
            {
                if (message.Exception.InnerException is ThrowHelpException)
                {
                    string helpCommand = $"grillhelp {ce.Context.Message.Content.Substring(1)}";
                    await Commands.ExecuteAsync(ce.Context, helpCommand, Services).ConfigureAwait(false);
                    return;
                }
                else if (message.Exception.InnerException is ConfigException)
                {
                    await ce.Context.Channel.SendMessageAsync("Nebyl definován platný config.").ConfigureAwait(false);
                    return;
                }
            }

            var exceptionMessage = message.Exception.ToString();
            var parts = exceptionMessage.SplitInParts(MessageSizeForException).ToArray();

            if (Client.GetChannel(LogRoom.Value) is IMessageChannel channel)
            {
                await SendLogMessageAsync(parts, channel).ConfigureAwait(false);
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

        private bool IsValidException(LogMessage message)
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

        public void Write(LogSeverity severity, string message, string source = "", Exception exception = null)
        {
            switch (severity)
            {
                case LogSeverity.Warning:
                    Logger.LogWarning($"{source}\t{message}");
                    break;
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Logger.LogCritical(exception, $"{source}\t{message}");
                    break;
                default:
                    Logger.LogInformation($"{source}\t{message}");
                    break;
            }
        }
    }
}
