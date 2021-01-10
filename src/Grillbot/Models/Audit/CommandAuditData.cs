using Discord.Commands;

namespace Grillbot.Models.Audit
{
    public class CommandAuditData
    {
        public string MessageContent { get; set; }
        public string Group { get; set; }
        public string CommandName { get; set; }

        public static CommandAuditData CreateDbItem(ICommandContext context, CommandInfo commandInfo)
        {
            return new CommandAuditData()
            {
                MessageContent = context.Message.Content,
                Group = commandInfo.Module.Group,
                CommandName = commandInfo.Name
            };
        }
    }
}