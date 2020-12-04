using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit
{
    public class CommandAuditData
    {
        [JsonIgnore]
        public IChannel Channel { get; set; }
        public ulong ChannelId { get; set; }

        public string MessageContent { get; set; }
        public string Group { get; set; }
        public string CommandName { get; set; }

        public static CommandAuditData CreateDbItem(ICommandContext context, CommandInfo commandInfo)
        {
            return new CommandAuditData()
            {
                ChannelId = context.Channel.Id,
                MessageContent = context.Message.Content,
                Group = commandInfo.Module.Group,
                CommandName = commandInfo.Name
            };
        }

        public CommandAuditData GetFilledModel(SocketGuild guild)
        {
            Channel = guild.GetChannel(ChannelId);

            return this;
        }
    }
}