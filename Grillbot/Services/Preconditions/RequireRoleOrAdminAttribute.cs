using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Preconditions
{
    public class RequireRoleOrAdminAttribute : PreconditionAttribute
    {
        public string RoleGroupName { get; set; }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var config = (IConfiguration)services.GetService(typeof(IConfiguration));

            var requiredRoles = config.GetSection($"MethodsConfig:{RoleGroupName}:RequireRoles").GetChildren().Select(o => o.Value).ToList();
            var allowedAdmins = config.GetSection($"Discord:Administrators").GetChildren().Select(o => o.Value).ToList();

            if (allowedAdmins.Contains(context.User.Id.ToString()))
                return Task.FromResult(PreconditionResult.FromSuccess());

            if(context.User is SocketGuildUser user)
            {
                foreach (var role in user.Roles)
                {
                    if (requiredRoles.Any(o => o == role.Name))
                        return Task.FromResult(PreconditionResult.FromSuccess());
                }
            }

            return Task.FromResult(PreconditionResult.FromError("Na tento příkaz nemáš dostatečnou roli."));
        }
    }
}
