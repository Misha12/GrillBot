using Discord;
using Discord.WebSocket;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        public async Task InsertMessageToCacheAsync(SocketUserMessage message)
        {
            if (!message.Attachments.Any(o => o.Width != null)) return;

            using (var repository = new LoggerCacheRepository(Config))
            {
                await repository.InserMessageToCacheAsync(message);
            }
        }

        public async Task SendAttachmentToLoggerRoomAsync(ulong messageID, bool deleteRecord = true)
        {
            if (string.IsNullOrEmpty(Config["Discord:LoggerRoomID"])) return;

            using (var repository = new LoggerCacheRepository(Config))
            {
                var message = await repository.GetMessageAsync(messageID);
                if (message == null) return;

                await SendToLoggerRoomAsync(message);

                if(deleteRecord)
                {
                    await repository.DeleteMessageFromCacheAsync(message);
                }
            }
        }

        private async Task SendToLoggerRoomAsync(LoggerMessage message)
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
                    Title = "Zpráva s přílohou byla odebrána.",
                    Color = Color.Red
                };

                if (author == null)
                    logEmbed.WithAuthor(o => o.WithName("Unknown user"));
                else
                    logEmbed.WithAuthor(o => o.WithName(author.Username).WithIconUrl(author.GetAvatarUrl()));

                logEmbed
                    .WithCurrentTimestamp()
                    .WithFooter(o =>o.WithText($"MessageID: {message.MessageID} | AuthorID: {message.AuthorID}"))
                    .AddField("Channel", $" <#{messageChannel.Id}> {messageChannel.Id}")
                    .AddField("Obsah", string.IsNullOrEmpty(message.Content) ? "-" : message.Content);

                if(message.Attachments.Count > 1)
                {
                    foreach (var attachment in message.Attachments)
                    {
                        var attachmentStream = await GetAttachmentStreamAsync(attachment);
                        if (attachmentStream.Item1 != null && attachmentStream.Item2 != null)
                            streams.Add(attachmentStream);
                    }
                }
                else
                {
                    var attachment = message.Attachments.First();

                    if (await IsSiteAvailableAsync(attachment.ProxyUrl))
                    {
                        logEmbed.WithImageUrl(attachment.ProxyUrl);
                    }
                    else if(await IsSiteAvailableAsync(attachment.UrlLink))
                    {
                        logEmbed.WithImageUrl(attachment.UrlLink);
                    }
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

        private async Task<Tuple<string, Stream>> GetAttachmentStreamAsync(LoggerAttachment attachment)
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

        private async Task<bool> IsSiteAvailableAsync(string url)
        {
            var request = WebRequest.CreateHttp(url);

            try
            {
                using (var response = (HttpWebResponse)(await request.GetResponseAsync()))
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch(WebException)
            {
                return false;
            }
        }
    }
}
