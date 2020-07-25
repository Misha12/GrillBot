using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Extensions;
using System.Net.WebSockets;
using Discord.Net;
using Microsoft.Extensions.Options;
using Grillbot.Exceptions;
using Microsoft.Extensions.Logging;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Services.Initiable;
using System.IO;
using System.Net.Sockets;
using System.Net.Http;

namespace Grillbot.Services
{
    public class BotLoggingService : IDisposable, IInitiable
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
            LogRoom = config.Value.Discord.ErrorLogChannelID;
            Services = services;
        }

        private async Task OnLogAsync(LogMessage message)
        {
            await PostException(message).ConfigureAwait(false);
            Write(message.Severity, message.Message, message.Source, message.Exception);
        }

        private async Task PostException(LogMessage message)
        {
            if (!CanSendExceptionToDiscord(message)) return;

            if (message.Exception is CommandException ce)
            {
                if (IsThrowHelpException(ce))
                {
                    string helpCommand = $"grillhelp {ce.Context.Message.Content.Substring(1)}";
                    await Commands.ExecuteAsync(ce.Context, helpCommand, Services).ConfigureAwait(false);
                    return;
                }

                if (IsConfigException(ce))
                {
                    await ce.Context.Channel.SendMessageAsync("Nebyl definován platný config");
                    return;
                }
            }

            var exceptionMessage = message.Exception.ToString();
            var parts = exceptionMessage.SplitInParts(MessageSizeForException).ToArray();

            if (Client.GetChannel(LogRoom.Value) is IMessageChannel channel)
            {
                foreach (var part in parts)
                {
                    await channel.SendMessageAsync($"```{part}```");
                }
            }
        }

        private bool CanSendExceptionToDiscord(LogMessage message) => message.Exception != null && LogRoom != null && !IsSupressedException(message.Exception);

        private bool IsSupressedException(Exception exception)
        {
            if (IsWebSocketException(exception)) return true;

            if (
                exception.InnerException == null
                && (
                    exception.Message.StartsWith("Server requested a reconnect", StringComparison.InvariantCultureIgnoreCase)
                    || exception.Message.StartsWith("Server missed last heartbeat", StringComparison.InvariantCultureIgnoreCase)
                )
            ) return true;

            if (
                (exception is TaskCanceledException || exception is HttpRequestException)
                && exception.InnerException is IOException iOException
                && iOException.InnerException is SocketException socketException
                && (new[] { SocketError.TimedOut, SocketError.OperationAborted }).Contains(socketException.SocketErrorCode)
            ) return true;

            return false;
        }

        public void Dispose()
        {
            Client.Log -= OnLogAsync;
            Commands.Log -= OnLogAsync;
        }

        public void Write(LogSeverity severity, string message, string source = "", Exception exception = null)
        {
            if (!CanLogException(exception)) return;

            switch (severity)
            {
                case LogSeverity.Warning when exception == null:
                    Logger.LogWarning($"{source}\t{message}");
                    break;
                case LogSeverity.Warning when exception != null:
                    Logger.LogWarning(exception, $"{source}\t{message}");
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

        private bool CanLogException(Exception exception)
        {
            if (exception == null)
                return true;

            if (exception is CommandException ce)
            {
                if (IsThrowHelpException(ce) || IsConfigException(ce))
                    return false;
            }

            return true;
        }

        private bool IsThrowHelpException(CommandException exception) => exception?.InnerException != null && exception.InnerException is ThrowHelpException;
        private bool IsConfigException(CommandException exception) => exception?.InnerException != null && exception.InnerException is ConfigException;
        private bool IsWebSocketException(Exception ex) => ex.InnerException != null && (ex.InnerException is WebSocketException || ex.InnerException is WebSocketClosedException);

        public void Init()
        {
            Client.Log += OnLogAsync;
            Commands.Log += OnLogAsync;
        }

        public async Task InitAsync() { }
    }
}
