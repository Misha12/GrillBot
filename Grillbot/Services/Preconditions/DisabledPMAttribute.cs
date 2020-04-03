using Discord.Commands;
using Grillbot.Messages.Services.Preconditions;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.Preconditions
{
    public class DisabledPMAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild == null)
                return Task.FromResult(PreconditionResult.FromError(DisabledPMAttributeMessages.Disabled));
            else
                return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
