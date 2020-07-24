using Discord.Commands;
using Grillbot.Enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.Permissions.Preconditions
{
    public class RequirePermissionsAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using var scope = services.CreateScope();
            using var permsManager = (PermissionsManager)scope.ServiceProvider.GetService(typeof(PermissionsManager));

            return Task.FromResult((permsManager.CheckPermissions(context, command)) switch
            {
                PermissionsResult.MethodNotFound => PreconditionResult.FromError("Tento příkaz nelze zpracovat. V konfiguraci chybí definice oprávnění."),
                PermissionsResult.MissingPermissions => PreconditionResult.FromError("Na tento příkaz nemáš dostatečnou roli."),
                PermissionsResult.OnlyAdmins => PreconditionResult.FromError("Tento příkaz je povolen pouze pro administrátory bota."),
                PermissionsResult.PMNotAllowed => PreconditionResult.FromError("Tento příkaz nelze provést v soukromé konverzaci."),
                PermissionsResult.UserIsBanned => PreconditionResult.FromError("Tento příkaz nemůžeš použít."),
                PermissionsResult.NoPermissions => PreconditionResult.FromError("Tento příkaz nemá nakonfigurované oprávnění."),
                _ => PreconditionResult.FromSuccess(),
            });
        }
    }
}
