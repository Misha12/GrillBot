using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WatchDog_Bot.Services
{
    public class RequireRoleAttribute : PreconditionAttribute
    {
        public string RoleGroupName { get; set; }
        
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var config = (IConfigurationRoot)services.GetService(typeof(IConfigurationRoot));
            var requiredRoles = config.GetSection($"MethodsConfig:{RoleGroupName}:RequireRoles").GetChildren().Select(o => o.Value).ToList();

            if(context.User is SocketGuildUser user)
            {
                foreach(var role in user.Roles)
                {
                    if (requiredRoles.Any(o => o == role.Name))
                        return Task.FromResult(PreconditionResult.FromSuccess());
                }
            }

            return Task.FromResult(PreconditionResult.FromError("Na tento příkaz nemáš dostatečnou roli."));
        }
    }
}
