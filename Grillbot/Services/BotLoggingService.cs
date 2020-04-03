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
using Grillbot.Messages;

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

        private async Task PostException(LogMessage message)
        {
            if (!CanSendExceptionToDiscord(message)) return;

            if(message.Exception is CommandException ce)
            {
                if (IsThrowHelpException(ce))
                {
                    string helpCommand = $"grillhelp {ce.Context.Message.Content.Substring(1)}";
                    await Commands.ExecuteAsync(ce.Context, helpCommand, Services).ConfigureAwait(false);
                    return;
                }

                if(IsConfigException(ce))
                {
                    await ce.Context.Channel.SendMessageAsync(BotLoggingServiceMessages.ConfigIsNotDefined).ConfigureAwait(false);
                    return;
                }
            }

            var exceptionMessage = message.Exception.ToString();
            var parts = exceptionMessage.SplitInParts(MessageSizeForException).ToArray();

            if (Client.GetChannel(LogRoom.Value) is IMessageChannel channel)
            {
                foreach(var part in parts)
                {
                    await channel.SendMessageAsync($"```{part}```");
                }
            }
        }

        public void ConfigChanged(Configuration newConfig)
        {
            Init(newConfig);
        }

        private bool CanSendExceptionToDiscord(LogMessage message)
        {
            return message.Exception != null && LogRoom != null && !IsSupressedException(message.Exception);
        }

        private bool IsSupressedException(Exception exception)
        {
            if (IsWebSocketException(exception)) return true;
            if (exception.InnerException == null && exception.Message.StartsWith("Server requested a reconnect", StringComparison.InvariantCultureIgnoreCase)) return true;

            return false;
        }

        private bool IsWebSocketException(Exception ex)
        {
            return ex.InnerException != null && (ex.InnerException is WebSocketException || ex.InnerException is WebSocketClosedException);
        }

        public void Dispose()
        {
            Client.Log -= OnLogAsync;
            Commands.Log -= OnLogAsync;
        }

        public void Write(LogSeverity severity, string message, string source = "", Exception exception = null)
        {
            if (exception is CommandException ce && IsThrowHelpException(ce)) return;

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

        private bool IsThrowHelpException(CommandException exception)
        {
            return exception != null && exception.InnerException != null && exception.InnerException is ThrowHelpException;
        }

        private bool IsConfigException(CommandException exception)
        {
            return exception != null && exception.InnerException != null && exception.InnerException is ConfigException;
        }
    }
}
