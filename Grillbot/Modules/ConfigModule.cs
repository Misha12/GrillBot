using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Database.Enums;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Grillbot.Models;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Services.Permissions.Preconditions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("config")]
    [RequirePermissions]
    [Name("Konfigurace bota")]
    [ModuleID("ConfigModule")]
    public class ConfigModule : BotModuleBase
    {
        public ConfigModule(IOptions<Configuration> options, ConfigRepository repository) : base(options, repository) { }

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

            var config = ConfigRepository.AddConfig(Context.Guild, command.Group, command.Command, adminsOnly, configJson);
            await ReplyAsync($"Konfigurační záznam `{command},OA:{adminsOnly},ID:{config.ID}` byl úspěšně přidán.");
        }

        [Command("listMethods")]
        [Summary("Seznam metod")]
        public async Task ListMethodsAsync()
        {
            var rows = new List<string>() { "ID\tSkupina/Příkaz\tOnlyAdmins" };

            var rowsData = ConfigRepository.GetAllMethods(Context.Guild)
                .Select(o => $"{o.ID}\t{o.Group}/{o.Command}\t{(o.OnlyAdmins ? 1 : 0)}");

            rows.AddRange(rowsData);

            await ReplyAsync($"```{string.Join("\n", rows)}```").ConfigureAwait(false);
        }

        [Command("switchOnlyAdmins")]
        [Summary("Přepne administrátorský režim pro metodu.")]
        public async Task SwitchOnlyAdminsAsync(GroupCommandMatch method, string onlyAdmins)
        {
            try
            {
                if (await CheckMissingMethodID(method)) return;

                ConfigRepository.UpdateMethod(Context.Guild, method.MethodID.Value, Convert.ToBoolean(onlyAdmins));
                await ReplyAsync("Příkaz byl úspěšně aktualizován.").ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("updateJsonConfig")]
        [Summary("Aktualizace configu")]
        public async Task UpdateJsonConfigAsync(GroupCommandMatch method, [Remainder] JObject jsonConfig)
        {
            if (await CheckMissingMethodID(method)) return;

            ConfigRepository.UpdateMethod(Context.Guild, method.MethodID.Value, jsonConfig: jsonConfig);
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

            try
            {
                ConfigRepository.AddPermission(Context.Guild, method.MethodID.Value, targetID, (PermType)permType, (AllowType)allowType);
                await ReplyAsync("Oprávnění bylo úspěšně přidáno.").ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("getMethod")]
        [Summary("Získání detailu metody vč. nastavených oprávnění.")]
        public async Task ListPermissionsAsync(GroupCommandMatch method)
        {
            if (await CheckMissingMethodID(method)) return;
            await Context.Guild.SyncGuildAsync().ConfigureAwait(false);

            var config = ConfigRepository.GetMethod(Context.Guild, method.MethodID.Value);
            await ReplyAsync($"ID: `{config.ID}`\nSkupina/Příkaz: `{config.Group}/{config.Command}`\nOnlyAdmins: `{config.OnlyAdmins}`").ConfigureAwait(false);

            var rowsData = config.Permissions.Select(o =>
            {
                switch (o.PermType)
                {
                    case PermType.Role:
                        var role = Context.Guild.GetRole(o.DiscordIDSnowflake);
                        return $"{o.PermID}\t{role.Name} ({role.Id})\t{o.PermType}\t{o.AllowType}";
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
                await ReplyAsync($"```{string.Join("\n", rowsData)}```").ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync("Tato metoda nemá nastaveny žádné oprávnění.").ConfigureAwait(false);
            }
        }

        [Command("removePermission")]
        [Summary("Smazání oprávnění.")]
        public async Task RemovePermissionAsync(GroupCommandMatch method, int permID)
        {
            if (await CheckMissingMethodID(method)) return;

            ConfigRepository.RemovePermission(Context.Guild, method.MethodID.Value, permID);
            await ReplyAsync("Oprávnění bylo odebráno").ConfigureAwait(false);
        }

        [Command("getJson")]
        [Summary("Získání aktuální JSON konfigurace dané metody.")]
        public async Task GetJsonConfig(GroupCommandMatch method)
        {
            if (await CheckMissingMethodID(method)) return;

            var config = ConfigRepository.GetMethod(Context.Guild, method.MethodID.Value);
            await ReplyAsync($"```json\n{config.ConfigData}```").ConfigureAwait(false);
        }

        [Command("removeMethod")]
        [Summary("Smazání metody")]
        public async Task RemoveMethodAsync(GroupCommandMatch method)
        {
            if (await CheckMissingMethodID(method)) return;

            try
            {
                ConfigRepository.RemoveMethod(Context.Guild.Id, method.MethodID.Value);
                await ReplyAsync("Metoda byla odebrána.");
            }
            catch(InvalidOperationException ex)
            {
                await ReplyAsync(ex.Message);
            }
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
    }
}
