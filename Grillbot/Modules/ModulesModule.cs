using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Database.Repository;
using Grillbot.Enums;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.PaginatedEmbed;
using Grillbot.Services;
using Grillbot.Services.Permissions.Preconditions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("modules")]
    [RequirePermissions]
    [Name("Správa modulů")]
    [ModuleID("ModulesModule")]
    public class ModulesModule : BotModuleBase
    {
        private CommandService CommandService { get; }
        private ILogger<ModulesModule> Logger { get; }
        private GlobalConfigRepository GlobalConfig { get; }

        public ModulesModule(CommandService commandService, GlobalConfigRepository globalConfig, PaginationService paginationService,
            ILogger<ModulesModule> logger) : base(paginationService: paginationService)
        {
            CommandService = commandService;
            Logger = logger;
            GlobalConfig = globalConfig;
        }

        [Command("list")]
        [Summary("Získání seznamu všech modulů.")]
        public async Task GetModulesAsync()
        {
            var moduleIdAttribute = typeof(ModuleIDAttribute);
            var unloadedModules = await GetUnloadedModulesAsync();
            var modulesChunk = CommandService.Modules
                .SplitInParts(EmbedBuilder.MaxFieldCount);

            var pages = new List<PaginatedEmbedPage>();

            foreach (var chunk in modulesChunk)
            {
                var page = new PaginatedEmbedPage(null);

                foreach (var item in chunk)
                {
                    var attribute = item.Attributes.FirstOrDefault(o => o.GetType() == moduleIdAttribute) as ModuleIDAttribute;

                    var info = item.Name + (string.IsNullOrEmpty(item.Group) ? "" : $" ({item.Group})");
                    page.AddField(attribute.ID, info + (unloadedModules.Contains(attribute.ID) ? " - **Deaktivován**" : ""), false);
                }

                if (page.AnyField())
                    pages.Add(page);
            }

            if (pages.Count == 0)
            {
                await ReplyAsync("Nebyl nalezen žádný modul.");
                return;
            }

            var embed = new PaginatedEmbed()
            {
                Title = "Seznam modulů",
                Pages = pages,
                ResponseFor = Context.User,
                Thumbnail = Context.Client.CurrentUser.GetUserAvatarUrl()
            };

            await SendPaginatedEmbedAsync(embed);
        }

        [Command("add")]
        [Summary("Přidání modulu.")]
        public async Task AddModuleAsync(string name)
        {
            var unloadedModules = await GetUnloadedModulesAsync();

            if (!unloadedModules.Contains(name))
            {
                await ReplyAsync("Tento modul je již aktivní.");
                return;
            }

            if(FindModule(name) == null)
            {
                await ReplyAsync("Tento modul nebyl nalezen.");
                return;
            }

            unloadedModules.Remove(name);
            Logger.LogInformation($"Requested add module {name}");
            await GlobalConfig.UpdateItemAsync(GlobalConfigItems.UnloadedModules, unloadedModules.Count == 0 ? null : JsonConvert.SerializeObject(unloadedModules));
            await ReplyAsync("Modul byl úspěšně povolen.");
        }

        [Command("remove")]
        [Summary("Deaktivace modulu.")]
        public async Task RemoveModuleAsync(string name)
        {
            var unloadedModules = await GetUnloadedModulesAsync();

            if (unloadedModules.Contains(name))
            {
                await ReplyAsync("Tento modul je již uvolněn.");
                return;
            }

            if (FindModule(name) == null)
            {
                await ReplyAsync("Tento modul nebyl nalezen.");
                return;
            }

            Logger.LogInformation($"Requested remove of module {name}");
            unloadedModules.Add(name);
            await GlobalConfig.UpdateItemAsync(GlobalConfigItems.UnloadedModules, JsonConvert.SerializeObject(unloadedModules));
            await ReplyAsync("Modul byl úspěšně uvolněn.");
        }

        private ModuleInfo FindModule(string name)
        {
            var moduleIdAttribute = typeof(ModuleIDAttribute);
            foreach (var module in CommandService.Modules)
            {
                var attribute = module.Attributes.FirstOrDefault(o => o.GetType() == moduleIdAttribute) as ModuleIDAttribute;

                if (attribute.ID == name)
                    return module;
            }

            return null;
        }

        private async Task<List<string>> GetUnloadedModulesAsync()
        {
            var unloadedModulesList = new List<string>();
            var unloadedModules = await GlobalConfig.GetItemAsync(GlobalConfigItems.UnloadedModules);

            if (!string.IsNullOrEmpty(unloadedModules))
                unloadedModulesList.AddRange(JsonConvert.DeserializeObject<List<string>>(unloadedModules));

            return unloadedModulesList;
        }
    }
}
