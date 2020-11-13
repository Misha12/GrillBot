using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Enums;
using Grillbot.Services.Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Permissions.Preconditions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleUnloadCheckAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var moduleIdAttribute = typeof(ModuleIDAttribute);

            if (command.Module.Attributes.FirstOrDefault(o => o.GetType() == moduleIdAttribute) is not ModuleIDAttribute moduleAttribute)
                return PreconditionResult.FromError("Tomuto modulu chybí attribute ModuleID");

            var isUnloaded = IsModuleUnloaded(moduleAttribute.ID, services);

            if (isUnloaded)
                return PreconditionResult.FromError($"Modul `{moduleAttribute.ID}` je deaktivován.");

            return PreconditionResult.FromSuccess();
        }

        private bool IsModuleUnloaded(string id, IServiceProvider services)
        {
            var unloadedModules = GetUnloadedModules(services);
            return unloadedModules.Contains(id);
        }

        private List<string> GetUnloadedModules(IServiceProvider services)
        {
            var unloadedModulesList = new List<string>();

            using var scope = services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<ConfigurationService>();

            var unloadedModules = repository.GetValue(GlobalConfigItems.UnloadedModules);
            if (!string.IsNullOrEmpty(unloadedModules))
                unloadedModulesList.AddRange(JsonConvert.DeserializeObject<List<string>>(unloadedModules));

            return unloadedModulesList;
        }
    }
}
