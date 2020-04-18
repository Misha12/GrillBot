using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Grillbot.Exceptions;
using Grillbot.Helpers;
using Grillbot.Modules.AutoReply;
using Grillbot.Services.AutoReply;
using Grillbot.Services.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Group("autoreply")]
    [Name("Ovládání automatických odpovědí")]
    public class AutoReplyModule : BotModuleBase
    {
        private AutoReplyService Service { get; }

        public AutoReplyModule(AutoReplyService service)
        {
            Service = service;
        }

        [Command("list")]
        [Summary("Vypíše všechny možné odpovědi.")]
        public async Task ListItemsAsync()
        {
            var data = Service.GetList();

            if (data.Count == 0)
                throw new BotCommandInfoException("Ještě nejsou uloženy žádné odpovědi.");

            var pages = new List<string>();
            foreach (var item in data)
            {
                pages.Add(string.Join("\n", new[] {
                    $"**{item.ID} - {item.MustContains}**",
                    $"Odpověď: {item.Reply}",
                    $"Status: {(item.IsActive ? "Aktivní" : "Neaktivní")}",
                    $"Metoda: {item.CompareType}",
                    $"Počet použití: {FormatHelper.FormatWithSpaces(item.CallsCount)}",
                    $"Case sensitive: {(item.CaseSensitive ? "Ano" : "Ne")}"
                }));
            }

            var paginated = new PaginatedMessage()
            {
                Pages = pages,
                Title = "Automatické odpovědi",
                Color = Color.Blue,
                Options = new PaginatedAppearanceOptions()
                {
                    DisplayInformationIcon = false,
                    Stop = null
                }
            };

            await PagedReplyAsync(paginated);
        }

        [Command("disable")]
        [Summary("Deaktivuje automatickou odpověď.")]
        public async Task DisableAsync(int id)
        {
            try
            {
                await Service.SetActiveStatusAsync(id, true);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně deaktivována.");
            }
            catch (ArgumentException ex)
            {
                throw new BotCommandInfoException(ex.Message);
            }
        }

        [Command("enable")]
        [Summary("Aktivuje automatickou odpověď.")]
        public async Task EnableAsync(int id)
        {
            try
            {
                await Service.SetActiveStatusAsync(id, false).ConfigureAwait(false);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně aktivována.").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new BotCommandInfoException(ex.Message);
            }
        }

        [Command("add")]
        [Summary("Přidá novou automatickou odpověď.")]
        [Remarks("Parametry jsou odděleny novým řádkem, očekávaný jsou parametry {MustContains}\\n{ReplyMessage}\\nTyp porovnání (==, Contains)\\n[{Příznaky}]\n" +
            "Příznaky: 1. bit: CaseSensitive, 2. bit: Deaktivovat")]
        public async Task AddAsync([Remainder] string data)
        {
            try
            {
                var fields = data.Split("\n").Where(o => !string.IsNullOrEmpty(o)).ToArray();

                if (fields.Length < 3)
                    throw new BotCommandInfoException("Nebyly zadány všechny potřebné parametry. (Musí obsahovat, Odpověď, Typ porovnání)");

                var paramsFlags = fields.Length > 3 ? Convert.ToInt32(fields[3]) : 0;
                var disabled = (paramsFlags & (int)AutoReplyParams.Disabled) != 0;
                var caseSensitive = (paramsFlags & (int)AutoReplyParams.CaseSensitive) != 0;

                await Service.AddReplyAsync(fields[0], fields[1], fields[2], disabled, caseSensitive).ConfigureAwait(false);
                await ReplyAsync($"Automatická odpověď **{fields[0]}** => **{fields[1]}** byla úspěšně přidána.").ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                throw new BotCommandInfoException(ex.Message);
            }
        }

        [Command("edit")]
        [Summary("Modifikuje existující automatickou odpověď.")]
        [Remarks("Parametry jsou téměř stejné jako u metody Add, jen s tím rozdílem, že je jako první parametr očekáváno ID.")]
        public async Task Edit(int id, [Remainder] string data)
        {
            try
            {
                var fields = data.Split("\n").Where(o => !string.IsNullOrEmpty(o)).ToArray();

                if (fields.Length < 3)
                    throw new BotCommandInfoException("Nebyly zadány všechny potřebné parametry.");

                var paramsFlags = fields.Length > 3 ? Convert.ToInt32(fields[3]) : 0;
                var caseSensitive = (paramsFlags & (int)AutoReplyParams.CaseSensitive) != 0;

                await Service.EditReplyAsync(id, fields[0], fields[1], fields[2], caseSensitive).ConfigureAwait(false);
                await ReplyAsync($"Automatická odpověď **{fields[0]}** => **{fields[1]}** byla úspěšně upravená.").ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                throw new BotCommandInfoException(ex.Message);
            }
        }

        [Command("remove")]
        [Summary("Odebere automatickou odpověď.")]
        public async Task Remove(int id)
        {
            try
            {
                await Service.RemoveReplyAsync(id).ConfigureAwait(false);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně odebrána.").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new BotCommandInfoException(ex.Message);
            }
        }
    }
}