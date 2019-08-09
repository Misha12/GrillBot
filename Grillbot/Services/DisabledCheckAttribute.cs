using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class DisabledCheckAttribute : PreconditionAttribute
    {
        public string RoleGroupName { get; set; }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            
            var config = (IConfiguration)services.GetService(typeof(IConfiguration));
            var isDisabled = config[$"MethodsConfig:{RoleGroupName}:IsDisabled"];

            if (string.IsNullOrEmpty(isDisabled) || !Convert.ToBoolean(isDisabled))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("Tento příkaz je deaktivován."));
        }
    }
}
