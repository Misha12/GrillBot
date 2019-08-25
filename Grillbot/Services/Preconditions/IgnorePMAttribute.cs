using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.Preconditions
{
    public class IgnorePMAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild == null)
                return Task.FromResult(PreconditionResult.FromError("Tento příkaz nelze použít v PM"));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
