using Discord.Commands;

namespace Grillbot.Models
{
    public class CommandRuntimeResult : RuntimeResult
    {
        public CommandRuntimeResult(CommandError? error, string reason) : base(error, reason)
        {
        }

        public static CommandRuntimeResult FromError(string reason)
        {
            return new CommandRuntimeResult(CommandError.Unsuccessful, reason);
        }

        public static CommandRuntimeResult FromSuccess(string reason = null)
        {
            return new CommandRuntimeResult(null, reason);
        }
    }
}
