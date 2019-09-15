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

        public RequirePermissionsAttribute(string permsGroupName)
        {
            PermsGroupName = permsGroupName;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var config = (IOptions<Configuration>)services.GetService(typeof(IOptions<Configuration>));
            var permissions = config.Value.MethodsConfig.GetPermissions(PermsGroupName);

            if(config.Value.Discord.IsUserBotAdmin(context.User.Id))
                return Task.FromResult(PreconditionResult.FromSuccess());

            if(context.User is SocketGuildUser user)
            {
                if (permissions.IsUserAllowed(context.User.Id))
                    return Task.FromResult(PreconditionResult.FromSuccess());

                if (permissions.IsUserBanned(context.User.Id))
                    return Task.FromResult(PreconditionResult.FromError("Tento příkaz nemůžeš použít."));

                foreach (var role in user.Roles)
                {
                    if (permissions.IsRoleAllowed(role.Name))
                        return Task.FromResult(PreconditionResult.FromSuccess());
                }
            }

            if (permissions.OnlyAdmins)
                return Task.FromResult(PreconditionResult.FromError("Tento příkaz je povolen pouze pro administrátory bota."));

            return Task.FromResult(PreconditionResult.FromError("Na tento příkaz nemáš dostatečnou roli."));
        }
    }
}
