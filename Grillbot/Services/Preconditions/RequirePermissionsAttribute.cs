using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.Preconditions
{
    public class RequirePermissionsAttribute : PreconditionAttribute
    {
        public string PermsGroupName { get; set; }
        public bool DisabledForPM { get; set; }
        public bool BoosterAllowed { get; set; }

        public RequirePermissionsAttribute(string permsGroupName)
        {
            PermsGroupName = permsGroupName;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild == null && DisabledForPM)
                return Task.FromResult(PreconditionResult.FromError("Tento příkaz nelpe provést v soukromé konverzaci."));

            var config = (IOptions<Configuration>)services.GetService(typeof(IOptions<Configuration>));
            var permissions = config.Value.MethodsConfig.GetPermissions(PermsGroupName);

            if (permissions == null)
                return Task.FromResult(PreconditionResult.FromError("Tento příkaz nelze zpracovat. V konfiguraci chybí definice oprávnění."));

            if (permissions.OnlyAdmins)
                return Task.FromResult(PreconditionResult.FromError("Tento příkaz je povolen pouze pro administrátory bota."));

            if (config.Value.IsUserBotAdmin(context.Message.Author.Id))
                return Task.FromResult(PreconditionResult.FromSuccess());

            if(context.Message.Author is SocketGuildUser user)
            {
                if (permissions.IsUserAllowed(user.Id))
                    return Task.FromResult(PreconditionResult.FromSuccess());

                if (permissions.IsUserBanned(user.Id))
                    return Task.FromResult(PreconditionResult.FromError("Tento příkaz nemůžeš použít."));

                if (config.Value.Discord.IsBooster(user.Roles) && BoosterAllowed)
                    return Task.FromResult(PreconditionResult.FromSuccess());

                foreach (var role in user.Roles)
                {
                    if (permissions.IsRoleAllowed(role.Name))
                        return Task.FromResult(PreconditionResult.FromSuccess());
                }
            }

            return Task.FromResult(PreconditionResult.FromError("Na tento příkaz nemáš dostatečnou roli."));
        }
    }
}
