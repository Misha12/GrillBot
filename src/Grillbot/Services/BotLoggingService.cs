using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Discord.Net;
using Grillbot.Exceptions;
using Microsoft.Extensions.Logging;
using Grillbot.Services.Initiable;
using System.IO;
using System.Net.Sockets;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Grillbot.Services.ErrorHandling;
using Grillbot.Services.Statistics.ApiStats;
using Grillbot.Services.Config;
using Grillbot.Database;
using Grillbot.Database.Entity;
using System.Text;

namespace Grillbot.Services
{
    public class BotLoggingService : IDisposable, IInitiable
    {
        public const int MessageSizeForException = 1980;

        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IServiceProvider Services { get; }
        private ILogger<BotLoggingService> Logger { get; }
        private ApiStatistics ApiStatistics { get; }
        private ConfigurationService ConfigurationService { get; }

        public ulong? LogRoomID
        {
            get
            {
                var id = ConfigurationService.GetValue(Enums.GlobalConfigItems.ErrorLogChannel);
                return string.IsNullOrEmpty(id) ? null : Convert.ToUInt64(id);
            }
        }

        public BotLoggingService(DiscordSocketClient client, CommandService commands, IServiceProvider services, ILogger<BotLoggingService> logger,
            ApiStatistics apiStatistics, ConfigurationService configurationService)
        {
            Client = client;
            Commands = commands;
            Logger = logger;
            Services = services;
            ApiStatistics = apiStatistics;
            ConfigurationService = configurationService;
        }

        public async Task OnLogAsync(LogMessage message)
        {
            using var scope = Services.CreateScope();

            Write(message.Severity, message.Message, message.Source, message.Exception);
            ApiStatistics.Increment(message);
            await PostException(message, scope.ServiceProvider).ConfigureAwait(false);
        }

        private async Task PostException(LogMessage message, IServiceProvider scopedProvider)
        {
            if (!CanSendExceptionToDiscord(message)) return;

            if (message.Exception is CommandException ce)
            {
                if (IsThrowHelpException(ce))
                {
                    string helpCommand = $"grillhelp {ce.Context.Message.Content[1..]}";
                    await Commands.ExecuteAsync(ce.Context, helpCommand, Services).ConfigureAwait(false);
                    return;
                }

                if (IsConfigException(ce))
                {
                    await ce.Context.Channel.SendMessageAsync("Nebyl definován platný config");
                    return;
                }
            }

            var repository = scopedProvider.GetService<IGrillBotRepository>();
            var logEmbedCreator = scopedProvider.GetService<LogEmbedCreator>();

            var entity = new ErrorLogItem()
            {
                CreatedAt = DateTime.Now,
                Data = $"{message.Source} {message.Message}\n{message.Exception}"
            };

            await repository.AddAsync(entity);
            await repository.CommitAsync();

            try
            {
                var logEmbed = logEmbedCreator.CreateErrorEmbed(message, entity);
                var channel = Client.GetChannel(LogRoomID.Value) as IMessageChannel;

                var contentBytes = Encoding.UTF8.GetBytes(message.Exception.ToString());
                using var ms = new MemoryStream(contentBytes);

                await channel.SendMessageAsync(embed: logEmbed.Build());
                await channel.SendFileAsync(ms, $"Exception_{DateTime.Now:O}.txt");
            }
            catch (Exception ex)
            {
                var errEntity = new ErrorLogItem()
                {
                    CreatedAt = DateTime.Now,
                    Data = ex.ToString()
                };

                await repository.AddAsync(errEntity);
                await repository.CommitAsync();
            }
        }

        private bool CanSendExceptionToDiscord(LogMessage message) => message.Exception != null && LogRoomID != null && !IsSupressedException(message.Exception);

        private bool IsSupressedException(Exception exception)
        {
            if (IsWebSocketException(exception)) return true;

            if (exception is GatewayReconnectException || (exception.InnerException is GatewayReconnectException)) return true;

            if (
                exception.InnerException == null
                && exception.Message.StartsWith("Server missed last heartbeat", StringComparison.InvariantCultureIgnoreCase)
            )
            {
                return true;
            }

            if (
                (exception is TaskCanceledException || exception is HttpRequestException)
                && exception.InnerException is IOException iOException
                && iOException.InnerException is SocketException socketException
                && (new[] { SocketError.TimedOut, SocketError.OperationAborted }).Contains(socketException.SocketErrorCode)
            )
            {
                return true;
            }

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

            return exception is not CommandException ce || (!IsThrowHelpException(ce) && !IsConfigException(ce));
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
