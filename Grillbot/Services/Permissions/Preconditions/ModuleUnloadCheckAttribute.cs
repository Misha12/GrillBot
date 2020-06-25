using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Database.Repository;
using Grillbot.Enums;
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

            if (!(command.Module.Attributes.FirstOrDefault(o => o.GetType() == moduleIdAttribute) is ModuleIDAttribute moduleAttribute))
                return PreconditionResult.FromError("Tomuto modulu chybí attribute ModuleID");

            var isUnloaded = await IsModuleUnloadedAsync(moduleAttribute.ID, services);

            if (isUnloaded)
                return PreconditionResult.FromError($"Modul `{moduleAttribute.ID}` je deaktivován.");

            return PreconditionResult.FromSuccess();
        }

        private async Task<bool> IsModuleUnloadedAsync(string id, IServiceProvider services)
        {
            var unloadedModules = await GetUnloadedModulesAsync(services);
            return unloadedModules.Contains(id);
        }

        private async Task<List<string>> GetUnloadedModulesAsync(IServiceProvider services)
        {
            var unloadedModulesList = new List<string>();

            using var scope = services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<GlobalConfigRepository>();

            var unloadedModules = await repository.GetItemAsync(GlobalConfigItems.UnloadedModules);
            if (!string.IsNullOrEmpty(unloadedModules))
                unloadedModulesList.AddRange(JsonConvert.DeserializeObject<List<string>>(unloadedModules));

            return unloadedModulesList;
        }
    }
}
