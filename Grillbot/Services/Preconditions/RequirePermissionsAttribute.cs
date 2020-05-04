using Discord.Commands;
using Grillbot.Services.Permissions;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.Preconditions
{
    public class RequirePermissionsAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using var permsManager = (PermissionsManager)services.GetService(typeof(PermissionsManager));

            return (permsManager.CheckPermissions(context, command)) switch
            {
                PermissionsResult.MethodNotFound => Task.FromResult(PreconditionResult.FromError("Tento příkaz nelze zpracovat. V konfiguraci chybí definice oprávnění.")),
                PermissionsResult.MissingPermissions => Task.FromResult(PreconditionResult.FromError("Na tento příkaz nemáš dostatečnou roli.")),
                PermissionsResult.OnlyAdmins => Task.FromResult(PreconditionResult.FromError("Tento příkaz je povolen pouze pro administrátory bota.")),
                PermissionsResult.PMNotAllowed => Task.FromResult(PreconditionResult.FromError("Tento příkaz nelpe provést v soukromé konverzaci.")),
                PermissionsResult.UserIsBanned => Task.FromResult(PreconditionResult.FromError("Tento příkaz nemůžeš použít.")),
                _ => Task.FromResult(PreconditionResult.FromSuccess()),
            };
        }
    }
}
