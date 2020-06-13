using Discord.Commands;
using Grillbot.Database.Enums;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
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
    public class ConfigModule : BotModuleBase
    {
        public ConfigModule(IOptions<Configuration> options, ConfigRepository repository) : base(options, repository) { }

        [Command("addMethod")]
        [Summary("Přidání metody do configu")]
        [Remarks("commandInfo parametr je dvojce parametrů {group}/{method}\nonlyAdmins mohou nabývat hodnot true/false\nconfigJson je konfigurační JSON.")]
        public async Task AddMethodAsync(string commandInfo, string onlyAdmins, [Remainder] JObject configJson)
        {
            if (!commandInfo.Contains("/"))
            {
                await ReplyAsync("Neplatný název skupina/metoda");
                return;
            }

            var groupAndCommand = commandInfo.Split('/');
            var adminsOnly = Convert.ToBoolean(onlyAdmins);

            var config = ConfigRepository.AddConfig(Context.Guild, groupAndCommand[0], groupAndCommand[1], adminsOnly, configJson);
            await ReplyAsync($"Konfigurační záznam `{commandInfo},OA:{adminsOnly},ID:{config.ID}` byl úspěšně přidán.");
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
        public async Task SwitchOnlyAdminsAsync(int methodID, string onlyAdmins)
        {
            try
            {
                ConfigRepository.UpdateMethod(Context.Guild, methodID, Convert.ToBoolean(onlyAdmins));
                await ReplyAsync("Příkaz byl úspěšně aktualizován.").ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("updateJsonConfig")]
        [Summary("Aktualizace configu")]
        public async Task UpdateJsonConfigAsync(int methodID, [Remainder] JObject jsonConfig)
        {
            ConfigRepository.UpdateMethod(Context.Guild, methodID, jsonConfig: jsonConfig);
            await ReplyAsync("Metoda byla úspěšně aktualizována").ConfigureAwait(false);
        }

        [Command("addPermission")]
        [Summary("Přidá oprávnění pro metodu.")]
        [Remarks("targetID je discord ID (Pokud chcete povolit všem, použijte klíčové slovo everyone." +
            "\nPermType specifikuje, co znamená ID (Role=0, User=1)\nAllowType znamená typ povolení (Allow=0, Deny=1)")]
        public async Task AddPermissionAsync(int methodID, string targetID, int permType, int allowType)
        {
            await Context.Guild.SyncGuildAsync().ConfigureAwait(false);

            if (!string.Equals(targetID, "everyone", StringComparison.InvariantCultureIgnoreCase))
            {
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
                    case PermType.Everyone:
                        await ReplyAsync("Pro povolení všem použij klíčové slovo `everyone` jako identifikátor.");
                        return;
                }
            }
            else
            {
                permType = (int)PermType.Everyone;
            }

            try
            {
                ConfigRepository.AddPermission(Context.Guild, methodID, targetID, (PermType)permType, (AllowType)allowType);
                await ReplyAsync("Oprávnění bylo úspěšně přidáno.").ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("listPermissions")]
        [Summary("Získá seznam oprávnění.")]
        public async Task ListPermissionsAsync(int methodID)
        {
            await Context.Guild.SyncGuildAsync().ConfigureAwait(false);

            var method = ConfigRepository.GetMethod(Context.Guild, methodID);
            await ReplyAsync($"ID: `{method.ID}`\nSkupina/Příkaz: `{method.Group}/{method.Command}`\nOnlyAdmins: `{method.OnlyAdmins}`").ConfigureAwait(false);

            var rowsData = method.Permissions.Select(o =>
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
        public async Task RemovePermissionAsync(int methodID, int permID)
        {
            ConfigRepository.RemovePermission(Context.Guild, methodID, permID);
            await ReplyAsync("Oprávnění bylo odebráno").ConfigureAwait(false);
        }

        [Command("getJsonConfig")]
        public async Task GetJsonConfig(int methodID)
        {
            var config = ConfigRepository.GetMethod(Context.Guild, methodID);
            await ReplyAsync($"```json\n {config.ConfigData}```").ConfigureAwait(false);
        }
    }
}
