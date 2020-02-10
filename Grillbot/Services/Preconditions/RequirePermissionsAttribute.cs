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
            var permsManager = (PermissionsManager)services.GetService(typeof(PermissionsManager));

            switch (permsManager.CheckPermissions(context, command))
            {
                case PermissionsResult.MethodNotFound:
                    return Task.FromResult(PreconditionResult.FromError("Tento příkaz nelze zpracovat. V konfiguraci chybí definice oprávnění."));
                case PermissionsResult.MissingPermissions:
                    return Task.FromResult(PreconditionResult.FromError("Na tento příkaz nemáš dostatečnou roli."));
                case PermissionsResult.OnlyAdmins:
                    return Task.FromResult(PreconditionResult.FromError("Tento příkaz je povolen pouze pro administrátory bota."));
                case PermissionsResult.PMNotAllowed:
                    return Task.FromResult(PreconditionResult.FromError("Tento příkaz nelpe provést v soukromé konverzaci."));
                case PermissionsResult.UserIsBanned:
                    return Task.FromResult(PreconditionResult.FromError("Tento příkaz nemůžeš použít."));
                default:
                    return Task.FromResult(PreconditionResult.FromSuccess());
            }
        }
    }
}
