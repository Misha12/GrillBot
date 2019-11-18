using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Models;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Logger.LoggerMethods.LogEmbed;
using Grillbot.Services.MessageCache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grillbot.Services.Logger.LoggerMethods
{
    public class MessageDeleted : LoggerMethodBase
    {
        public MessageDeleted(DiscordSocketClient client, Configuration config, IMessageCache messageCache, HttpClient httpClient,
            BotLoggingService loggingService, TopStack stack) : base(client, config, messageCache, httpClient, loggingService, stack)
        {
        }

        public async Task ProcessAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var deletedMessage = message.Value;

            if (!message.HasValue)
                deletedMessage = MessageCache.TryRemove(message.Id);

            if (deletedMessage != null)
                await ProcessWithCacheRecordAsync(deletedMessage, channel);
            else
                await ProcessWithoutCacheRecordAsync(message.Id, channel);

            if (MessageCache.Exists(message.Id))
                MessageCache.TryRemove(message.Id);
        }

        private async Task ProcessWithoutCacheRecordAsync(ulong messageId, ISocketMessageChannel channel)
        {
            var logEmbedBuilder = new LogEmbedBuilder("Zpráva byla smazána.", LogEmbedType.MessageDeleted);

            logEmbedBuilder
                .SetTimestamp(true)
                .SetFooter($"MessageID: {messageId}")
                .SetTitle("Zpráva nebyla nalezena v cache.")
                .AddField("Kanál", $"<#{channel.Id}> ({channel.Id})");

            var loggerRoom = GetLoggerRoom();
            var result = await loggerRoom.SendMessageAsync(embed: logEmbedBuilder.Build());

            TopStack.Add(result);
        }

        private async Task<IAuditLogEntry> GetAuditLogRecord(ISocketMessageChannel channel, ulong authorID)
        {
            if (channel is SocketGuildChannel socketGuildChannel)
            {
                var logs = await GetAuditLogDataAsync(socketGuildChannel.Guild);
                var messageDeletedRecords = logs.Where(o => o.Action == ActionType.MessageDeleted).ToList();

                return messageDeletedRecords.FirstOrDefault(o =>
                {
                    var data = (MessageDeleteAuditLogData)o.Data;
                    return data.AuthorId == authorID && data.ChannelId == channel.Id;
                });
            }

            return null;
        }

        private async Task<List<RestAuditLogEntry>> GetAuditLogDataAsync(SocketGuild guild)
        {
            try
            {
                return (await guild.GetAuditLogsAsync(5).FlattenAsync()).ToList();
            }
            catch (Exception ex)
            {
                await LoggingService.WriteToLogAsync(ex.ToString());
                return new List<RestAuditLogEntry>();
            }
        }

        private async Task ProcessWithCacheRecordAsync(IMessage message, ISocketMessageChannel channel)
        {
            var auditLogRecord = await GetAuditLogRecord(channel, message.Author.Id);
            var streams = new List<Tuple<string, Stream>>();

            try
            {
                var logEmbedBuilder = new LogEmbedBuilder("Zpráva byla odebrána", LogEmbedType.MessageDeleted);

                logEmbedBuilder
                    .SetAuthor(message.Author, true)
                    .SetTimestamp(true)
                    .SetFooter($"MessageID: {message.Id} | AuthorID: {message.Author?.Id}")
                    .AddField("Odesláno v", message.CreatedAt.LocalDateTime.ToLocaleDatetime())
                    .AddField("Kanál", $"<#{message.Channel.Id}> ({message.Channel.Id})");


                if (auditLogRecord != null)
                    logEmbedBuilder.AddField("Smazal", $"{auditLogRecord.User.Username}#{auditLogRecord.User.Discriminator}");
                else
                    logEmbedBuilder.AddField("Smazal", $"{message.Author.Username}#{message.Author.Discriminator}");

                if (string.IsNullOrEmpty(message.Content))
                    logEmbedBuilder.AddField("Obsah", "-");
                else
                    logEmbedBuilder.AddCodeBlockField("Obsah", message.Content);

                if (message.Attachments.Any())
                {
                    foreach (var messageAttachment in message.Attachments)
                    {
                        var attachmentStream = await GetAttachmentStreamAsync(messageAttachment);
                        if (!string.IsNullOrEmpty(attachmentStream.Item1) && attachmentStream.Item2 != null)
                            streams.Add(attachmentStream);
                    }
                }

                var loggerChannel = GetLoggerRoom();
                var result = await loggerChannel.SendMessageAsync(embed: logEmbedBuilder.Build());
                foreach (var stream in streams)
                {
                    await loggerChannel.SendFileAsync(stream.Item2, stream.Item1);
                }

                var info = $"{message.Author.Username}#{message.Author.Discriminator}";
                TopStack.Add(result, info);
            }
            finally
            {
                streams.ForEach(o => o.Item2.Dispose());
            }
        }

        private async Task<Tuple<string, Stream>> GetAttachmentStreamAsync(IAttachment attachment)
        {
            Stream stream;

            try
            {
                var filename = CreateAttachmentFilename(attachment.Id, attachment.Url);
                stream = await HttpClient.GetStreamAsync(attachment.Url);
                return new Tuple<string, Stream>(filename, stream);
            }
            catch (HttpRequestException)
            {
                try
                {
                    var filename = CreateAttachmentFilename(attachment.Id, attachment.ProxyUrl);
                    stream = await HttpClient.GetStreamAsync(attachment.ProxyUrl);
                    return new Tuple<string, Stream>(filename, stream);
                }
                catch (HttpRequestException)
                {
                    return new Tuple<string, Stream>(null, null);
                }
            }
        }

        private string CreateAttachmentFilename(ulong id, string url)
        {
            return $"Attachment_{id}_{Path.GetExtension(url)}";
        }
    }
}
