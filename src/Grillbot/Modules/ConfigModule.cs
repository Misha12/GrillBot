using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Database;
using Grillbot.Database.Entity.MethodConfig;
using Grillbot.Database.Enums;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("config")]
    [Name("Konfigurace bota")]
    [ModuleID(nameof(ConfigModule))]
    public class ConfigModule : BotModuleBase
    {
        public ConfigModule(PaginationService paginationService, IServiceProvider provider) : base(paginationService: paginationService, provider: provider) { }

        [Command("addMethod")]
        [Summary("Přidání metody do configu")]
        [Remarks("command parametr je dvojce parametrů {group}/{method}\nonlyAdmins mohou nabývat hodnot true/false\nconfigJson je konfigurační JSON.")]
        public async Task AddMethodAsync(GroupCommandMatch command, string onlyAdmins, [Remainder] JObject configJson)
        {
            if (command.MethodID != null)
            {
                await ReplyAsync("Tato konfigurace již existuje.");
                return;
            }

            var adminsOnly = Convert.ToBoolean(onlyAdmins);
            var config = MethodsConfig.Create(Context.Guild, command.Group, command.Command, adminsOnly, configJson);

            using var service = GetService<IGrillBotRepository>();
            await service.Service.AddAsync(config);
            await service.Service.CommitAsync();

            await ReplyAsync($"Konfigurační záznam `{command},OA:{adminsOnly},ID:{config.ID}` byl úspěšně přidán.");
        }

        [Command("list")]
        [Summary("Seznam metod")]
        public async Task ListMethodsAsync()
        {
            var embed = new PaginatedEmbed()
            {
                Pages = new List<PaginatedEmbedPage>(),
                ResponseFor = Context.User,
                Thumbnail = Context.Client.CurrentUser.GetUserAvatarUrl(),
                Title = "Konfigurace metod"
            };

            using var service = GetService<IGrillBotRepository>();
            var methods = await service.Service.ConfigRepository.GetAllMethods(Context.Guild.Id, true).ToListAsync();

            foreach (var group in methods.GroupBy(o => o.Group))
            {
                var page = new PaginatedEmbedPage(null);

                foreach (var method in group)
                {
                    var value = string.Join("\n", new[]
                    {
                        $"ID: **{method.ID}**",
                        $"Počet oprávnění: **{method.Permissions.Count.FormatWithSpaces()}**",
                        $"Počet použití: **{method.UsedCount.FormatWithSpaces()}**"
                    });

                    page.AddField($"{method.Group}/{method.Command}", value);
                }

                if (page.AnyField())
                    embed.Pages.Add(page);
            }

            await SendPaginatedEmbedAsync(embed);
        }

        [Command("switchOnlyAdmins")]
        [Summary("Přepne administrátorský režim pro metodu.")]
        public async Task SwitchOnlyAdminsAsync(GroupCommandMatch method, string onlyAdmins)
        {
            if (await CheckMissingMethodID(method)) return;

            using var service = GetService<IGrillBotRepository>();
            var config = await service.Service.ConfigRepository.FindConfigAsync(Context.Guild.Id, method.Group, method.Command, false);

            if (config == null)
            {
                await ReplyAsync("Požadovaná metoda neexistuje.");
                return;
            }

            config.OnlyAdmins = Convert.ToBoolean(onlyAdmins);
            await service.Service.CommitAsync();

            await ReplyAsync("Příkaz byl úspěšně aktualizován.").ConfigureAwait(false);
        }

        [Command("updateJson")]
        [Summary("Aktualizace configu")]
        public async Task UpdateJsonConfigAsync(GroupCommandMatch method, [Remainder] JObject jsonConfig)
        {
            if (await CheckMissingMethodID(method)) return;

            using var service = GetService<IGrillBotRepository>();

            var config = await service.Service.ConfigRepository.FindConfigAsync(Context.Guild.Id, method.Group, method.Command, false);

            if (config == null)
            {
                await ReplyAsync("Požadovaná metoda neexistuje.");
                return;
            }

            config.Config = jsonConfig;
            await service.Service.CommitAsync();

            await ReplyAsync("Metoda byla úspěšně aktualizována").ConfigureAwait(false);
        }

        [Command("addPermission")]
        [Summary("Přidá oprávnění pro metodu.")]
        [Remarks("targetID je discord ID (Pokud chcete povolit všem, použijte klíčové slovo everyone." +
            "\nPermType specifikuje, co znamená ID (Role=0, User=1)\nAllowType znamená typ povolení (Allow=0, Deny=1)")]
        public async Task AddPermissionAsync(GroupCommandMatch method, string targetID, int permType, int allowType)
        {
            if (await CheckMissingMethodID(method)) return;

            await Context.Guild.SyncGuildAsync();

            if (string.Equals(targetID, "everyone", StringComparison.InvariantCultureIgnoreCase))
            {
                permType = (int)PermType.Everyone;
            }

            var id = Convert.ToUInt64(targetID);
            switch ((PermType)permType)
            {
                case PermType.Role:
                    if (Context.Guild.GetRole(id) == null)
                    {
                        await ReplyAsync("Taková role neexistuje");
                        return;
                    }
                    break;
                case PermType.User:
                    if (await Context.Guild.GetUserFromGuildAsync(id) == null)
                    {
                        await ReplyAsync("Takový uživatel neexistuje.");
                        return;
                    }
                    break;
                case PermType.Everyone when !string.Equals(targetID, "everyone", StringComparison.InvariantCultureIgnoreCase):
                    await ReplyAsync("Pro povolení všem použij klíčové slovo `everyone` jako identifikátor.");
                    return;
            }

            using var service = GetService<IGrillBotRepository>();
            var config = await service.Service.ConfigRepository.FindConfigAsync(Context.Guild.Id, method.Group, method.Command, true);

            if (config == null)
            {
                await ReplyAsync("Požadovaná metoda neexistuje.");
                return;
            }

            config.Permissions.Add(new MethodPerm()
            {
                AllowType = (AllowType)allowType,
                DiscordID = targetID,
                PermType = (PermType)permType
            });

            await service.Service.CommitAsync();
            await ReplyAsync("Oprávnění bylo úspěšně přidáno.");
        }

        [Command("getMethod")]
        [Summary("Získání detailu metody vč. nastavených oprávnění.")]
        public async Task ListPermissionsAsync(GroupCommandMatch method)
        {
            if (await CheckMissingMethodID(method)) return;
            await Context.Guild.SyncGuildAsync();

            using var service = GetService<IGrillBotRepository>();
            var config = await service.Service.ConfigRepository.FindConfigAsync(Context.Guild.Id, method.Group, method.Command, true);
            await ReplyAsync($"ID: `{config.ID}`\nSkupina/Příkaz: `{config.Group}/{config.Command}`\nOnlyAdmins: `{config.OnlyAdmins}`");

            var rowsData = config.Permissions.Select(o =>
            {
                switch (o.PermType)
                {
                    case PermType.Role:
                        var role = Context.Guild.GetRole(o.DiscordIDSnowflake);
                        return $"{o.PermID}\t{role?.Name ?? "Neznámá role"} ({role?.Id ?? o.DiscordIDSnowflake})\t{o.PermType}\t{o.AllowType}";
                    case PermType.User:
                        var user = Context.Guild.GetUserFromGuildAsync(o.DiscordID).Result;
                        return $"{o.PermID}\t{user.GetFullName()}\t{o.PermType}\t{o.AllowType}";
                    case PermType.Everyone:
                        return $"{o.PermID}\tEveryone\t-\t{o.AllowType}";
                }

                return null;
            }).Where(o => o != null).ToList();

            if (rowsData.Count > 0)
            {
                rowsData.Insert(0, "ID\tUživatel/Role\tTyp práva\tTyp povolení");
                await ReplyAsync($"```{string.Join("\n", rowsData)}```");
            }
            else
            {
                await ReplyAsync("Tato metoda nemá nastaveny žádné oprávnění.");
            }
        }

        [Command("removePermission")]
        [Summary("Smazání oprávnění.")]
        public async Task RemovePermissionAsync(GroupCommandMatch method, int permID)
        {
            if (await CheckMissingMethodID(method)) return;

            using var service = GetService<IGrillBotRepository>();

            var config = await service.Service.ConfigRepository.FindConfigAsync(Context.Guild.Id, method.Group, method.Command, true);

            if (config == null)
            {
                await ReplyAsync("Požadovaná metoda neexistuje");
                return;
            }

            var permission = config.Permissions.FirstOrDefault(o => o.PermID == permID);

            if (permission == null)
            {
                await ReplyAsync("Požadované oprávnění neexistuje.");
                return;
            }

            config.Permissions.Remove(permission);
            await service.Service.CommitAsync();
            await ReplyAsync("Oprávnění bylo odebráno");
        }

        [Command("getJson")]
        [Summary("Získání aktuální JSON konfigurace dané metody.")]
        public async Task GetJsonConfig(GroupCommandMatch method)
        {
            if (await CheckMissingMethodID(method)) return;

            using var service = GetService<IGrillBotRepository>();
            var config = await service.Service.ConfigRepository.FindConfigAsync(Context.Guild.Id, method.Group, method.Command, false);

            if (config == null)
            {
                await ReplyAsync("Požadovaná metoda neexistuje.");
                return;
            }

            if (config.ConfigData.Length >= DiscordConfig.MaxMessageSize - 11)
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(config.ConfigData));
                await Context.Channel.SendFileAsync(stream, $"{method.Group}_{method.Command}.json");
                return;
            }

            await ReplyAsync($"```json\n{config.ConfigData}```").ConfigureAwait(false);
        }

        [Command("removeMethod")]
        [Summary("Smazání metody")]
        public async Task RemoveMethodAsync(GroupCommandMatch method)
        {
            if (await CheckMissingMethodID(method)) return;

            using var service = GetService<IGrillBotRepository>();
            var config = await service.Service.ConfigRepository.FindConfigAsync(Context.Guild.Id, method.Group, method.Command, true);

            if (config == null)
            {
                await ReplyAsync("Požadovaná metoda neexistuje.");
                return;
            }

            service.Service.Remove(config);
            await service.Service.CommitAsync();
            await ReplyAsync("Metoda byla odebrána.");
        }

        private async Task<bool> CheckMissingMethodID(GroupCommandMatch command)
        {
            if (command.MethodID == null)
            {
                await ReplyAsync($"Hledaná konfigurace pro metodu `{command}` nebyla nalezena.");
                return true;
            }

            return false;
        }

        [Command("removeGuild")]
        [Summary("Smazání všech konfigurací o serveru")]
        public async Task RemoveGuildAsync(ulong guildID)
        {
            using var service = GetService<IGrillBotRepository>();

            var configs = await service.Service.ConfigRepository.GetAllMethods(guildID, true).ToListAsync();

            if (configs.Count > 0)
            {
                service.Service.RemoveCollection(configs);
                await service.Service.CommitAsync();
            }

            await ReplyAsync($"Guild `{guildID}` byla úspěšně z databáze uklizena.");
        }

        [Command("export")]
        [Summary("Kompletní export konfigurace do JSON souboru.")]
        public async Task ExportConfigurationAsync()
        {
            using var service = GetService<IGrillBotRepository>();
            var configs = await service.Service.ConfigRepository.GetAllMethods(Context.Guild.Id, true).ToListAsync();
            var json = JsonConvert.SerializeObject(configs);

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));

            await Context.User.SendFileAsync(ms, $"{Context.Guild.Name}.json");
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("import")]
        [Summary("Import dat do konfigurace.")]
        [Remarks("Tato metoda vyžaduje soubor jako přílohu.")]
        public async Task ImportConfigurationAsync()
        {
            var attachment = Context.Message.Attachments.FirstOrDefault(o => Path.GetExtension(o.Filename) == ".json");

            if (attachment == null)
            {
                await ReplyAsync("Nebyl nalezen žádný JSON soubor.");
                return;
            }

            var bytes = await attachment.DownloadFileAsync();
            var jsonData = Encoding.UTF8.GetString(bytes);
            var importedData = JsonConvert.DeserializeObject<List<MethodsConfig>>(jsonData);

            var state = Context.Channel.EnterTypingState();
            try
            {
                using var service = GetService<IGrillBotRepository>();

                foreach (var method in importedData)
                {
                    if (await service.Service.ConfigRepository.ConfigExistsAsync(method.GuildIDSnowflake, method.Group, method.Command))
                    {
                        await ReplyAsync($"> Metoda `{method}` již existuje. **Ignoruji!**");
                        continue;
                    }

                    var entity = MethodsConfig.Create(method.GuildIDSnowflake, method.Group, method.Command, method.OnlyAdmins, method.Config);

                    foreach (var perm in method.Permissions)
                    {
                        entity.Permissions.Add(new MethodPerm()
                        {
                            AllowType = perm.AllowType,
                            DiscordID = perm.DiscordID,
                            PermType = perm.PermType
                        });
                    }

                    await service.Service.AddAsync(entity);
                    await ReplyAsync($"> Import metody `{method}` připraven.");
                }

                await service.Service.CommitAsync();
            }
            finally
            {
                state.Dispose();
            }

            await ReplyAsync("Uložení bylo úspěšné. Data importována.");
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("rename")]
        [Summary("Přejmenování metody")]
        public async Task RenameMethod(int id, string group, string command)
        {
            using var service = GetService<IGrillBotRepository>();

            var config = await service.Service.ConfigRepository.GetAllMethods(Context.Guild.Id, false)
                .SingleOrDefaultAsync(o => o.ID == id);

            if (config == null)
            {
                await ReplyAsync("Požadovaná metoda neexistuje.");
                return;
            }

            config.Group = group;
            config.Command = command;
            await service.Service.CommitAsync();
            await ReplyAsync("Metoda byla přejmenována.");
        }
    }
}
