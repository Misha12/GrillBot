using Discord;
using Discord.WebSocket;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class LoggerCache : IConfigChangeable
    {
        private IConfiguration Config { get; set; }
        private DiscordSocketClient Client { get; }

        public LoggerCache(IConfiguration config, DiscordSocketClient client)
        {
            ConfigChanged(config);
            Client = client;
        }

        public void ConfigChanged(IConfiguration newConfig)
        {
            Config = newConfig;
        }

        public async Task InsertMessageToCache(SocketUserMessage message)
        {
            if (!message.Attachments.Any()) return;

            using (var repository = new LoggerCacheRepository(Config))
            {
                await repository.InserMessageToCache(message);
            }
        }

        public async Task SendAttachmentToLoggerRoom(ulong messageID, bool deleteRecord = true)
        {
            if (string.IsNullOrEmpty(Config["Discord:LoggerRoomID"])) return;

            using (var repository = new LoggerCacheRepository(Config))
            {
                var message = await repository.GetMessage(messageID);
                if (message == null) return;

                await SendToLoggerRoom(message);

                if(deleteRecord)
                {
                    await repository.DeleteMessageFromCache(message);
                }
            }
        }

        private async Task SendToLoggerRoom(LoggerMessage message)
        {
            var streams = new List<Tuple<string, Stream>>();

            try
            {
                var loggerChannelID = Convert.ToUInt64(Config["Discord:LoggerRoomID"]);
                if (!(Client.GetChannel(loggerChannelID) is ISocketMessageChannel loggerChannel))
                    return;

                var author = Client.GetUser(message.SnowflakeAuthorID);
                var messageChannel = Client.GetChannel(message.SnowflakeChannelID) as ISocketMessageChannel;

                var logEmbed = new EmbedBuilder()
                {
                    Title = "Zpráva s přílohou byla odebrána."
                };

                if (author == null)
                    logEmbed.WithAuthor(o => o.WithName("Unknown user"));
                else
                    logEmbed.WithAuthor(o => o.WithName(author.Username).WithIconUrl(author.GetAvatarUrl()));

                logEmbed
                    .WithCurrentTimestamp()
                    .AddField("Channel", messageChannel.Name + $" <#{messageChannel.Id}>");

                if (!string.IsNullOrEmpty(message.Content))
                    logEmbed.AddField("Obsah", message.Content);

                foreach (var attachment in message.Attachments)
                {
                    var attachmentStream = await GetAttachmentStream(attachment);
                    if (attachmentStream.Item1 != null && attachmentStream.Item2 != null)
                        streams.Add(attachmentStream);
                }

                await loggerChannel.SendMessageAsync(embed: logEmbed.Build());
                foreach (var stream in streams)
                {
                    await loggerChannel.SendFileAsync(stream.Item2, stream.Item1);
                }
            }
            finally
            {
                foreach (var stream in streams)
                    stream.Item2.Dispose();
            }
        }

        private async Task<Tuple<string, Stream>> GetAttachmentStream(LoggerAttachment attachment)
        {
            Stream stream;

            try
            {
                using (var client = new HttpClient())
                {
                    var filename = $"Attachment_{attachment.AttachmentID}.{Path.GetExtension(attachment.UrlLink)}";
                    stream = await client.GetStreamAsync(attachment.UrlLink);
                    return new Tuple<string, Stream>(filename, stream);
                }
            }
            catch (HttpRequestException)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        var filename = $"Attachment_{attachment.AttachmentID}.{Path.GetExtension(attachment.ProxyUrl)}";
                        stream = await client.GetStreamAsync(attachment.ProxyUrl);
                        return new Tuple<string, Stream>(filename, stream);
                    }
                }
                catch (HttpRequestException)
                {
                    return new Tuple<string, Stream>(null, null);
                }
            }
        }
    }
}
