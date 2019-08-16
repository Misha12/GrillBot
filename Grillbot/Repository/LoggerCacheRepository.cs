using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Helpers;
using Grillbot.Models;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Repository
{
    public class LoggerCacheRepository : RepositoryBase
    {
        public LoggerCacheRepository(IConfiguration config) : base(config)
        {
        }

        public async Task InsertMessage(SocketUserMessage message)
        {
            var commandBuilder = new List<SqlCommand>();

            try
            {
                var messageInsertCommand = new SqlCommand(
                "INSERT INTO LoggerMessageCache ([MessageID], [AuthorID], [Content], [CreatedAt], [ChannelID]) VALUES " +
                "(@messageID, @authorID, @content, @createdAt, @channelID)");

                messageInsertCommand.Parameters.AddWithValue("@messageID", message.Id.ToString());
                messageInsertCommand.Parameters.AddWithValue("@authorID", message.Author.Id.ToString());
                messageInsertCommand.Parameters.AddWithValue("@content", message.Content);
                messageInsertCommand.Parameters.AddWithValue("@createdAt", message.CreatedAt);
                messageInsertCommand.Parameters.AddWithValue("@channelID", message.Channel.Id.ToString());
                commandBuilder.Add(messageInsertCommand);

                commandBuilder.AddRange(message.Attachments.Select(o =>
                {
                    var cmd = new SqlCommand("INSERT INTO LoggerAttachmentCache ([AttachmentID], [MessageID], [UrlLink], [ProxyUrl]) VALUES " +
                        "(@attachmentID, @messageID, @urlLink, @proxyUrl)");

                    cmd.Parameters.AddWithValue("@attachmentID", o.Id.ToString());
                    cmd.Parameters.AddWithValue("@messageID", message.Id.ToString());
                    cmd.Parameters.AddWithValue("@urlLink", o.Url);
                    cmd.Parameters.AddWithValue("@proxyUrl", o.ProxyUrl);

                    return cmd;
                }));

                await ExecuteNonReaderBatch(commandBuilder);
            }
            finally
            {
                foreach (var command in commandBuilder)
                    command.Dispose();
            }
        }

        public async Task<LoggerUserMessage> GetMessage(ulong messageID)
        {
            var query = "SELECT l.MessageID, l.AuthorID, l.Content, l.CreatedAt, la.UrlLink, la.ProxyUrl, la.AttachmentID, l.ChannelID FROM " +
                "LoggerMessageCache l JOIN LoggerAttachmentCache la ON l.MessageID = la.MessageID WHERE l.MessageID=@messageID;";

            var command = new SqlCommand(query);
            command.Parameters.AddWithValue("@messageID", messageID.ToString());

            return await ExecuteCommand(query, async o =>
            {
                if (!o.HasRows)
                    return null;

                var message = new LoggerUserMessage();

                while(await o.ReadAsync())
                {
                    unchecked
                    {
                        message.SetValues(o["AuthorID"], o["MessageID"], o["Content"], o["CreatedAt"], o["ChannelID"]);
                        message.AddAttachment(o["AttachmentID"], o["UrlLink"], o["ProxyUrl"]);
                    }
                }

                return message;
            }, new SqlParameter("@messageID", messageID.ToString()));
        }

        public async Task DeleteMessageFromCache(ulong messageID)
        {
            var commandBuilder = new List<SqlCommand>();

            var deleteAttachments = new SqlCommand("DELETE FROM LoggerAttachmentCache WHERE MessageID=@messageId");
            deleteAttachments.Parameters.AddWithValue("@messageId", messageID.ToString());
            commandBuilder.Add(deleteAttachments);

            var deleteMessages = new SqlCommand("DELETE FROM LoggerMessageCache WHERE MessageID=@messageId");
            deleteMessages.Parameters.AddWithValue("@messageId", messageID.ToString());
            commandBuilder.Add(deleteMessages);

            await ExecuteNonReaderBatch(commandBuilder);
        }
    }
}
