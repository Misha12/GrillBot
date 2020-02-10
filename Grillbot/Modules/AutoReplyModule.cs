using Discord.Commands;
using Grillbot.Modules.AutoReply;
using Grillbot.Services.AutoReply;
using Grillbot.Services.Preconditions;
using System;
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
            var data = Service.GetList(Context.Message);

            if (data != null)
                await ReplyAsync(embed: data).ConfigureAwait(false);
            else
                await ReplyAsync("Ještě nejsou uloženy žádné odpověďi.").ConfigureAwait(false);
        }

        [Command("disable")]
        [Summary("Deaktivuje automatickou odpověď.")]
        public async Task DisableAsync(int id)
        {
            await DoAsync(async () =>
            {
                await Service.SetActiveStatusAsync(id, true).ConfigureAwait(false);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně deaktivována.").ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("enable")]
        [Summary("Aktivuje automatickou odpověď.")]
        public async Task EnableAsync(int id)
        {
            await DoAsync(async () =>
            {
                await Service.SetActiveStatusAsync(id, false).ConfigureAwait(false);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně aktivována.").ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("add")]
        [Summary("Přidá novou automatickou odpověď.")]
        [Remarks("Parametry jsou odděleny novým řádkem, očekávaný jsou parametry {MustContains}\\n{ReplyMessage}\\nTyp porovnání (==, Contains)\\n[{Příznaky}]\n" +
            "Příznaky: 1. bit: CaseSensitive, 2. bit: Deaktivovat")]
        public async Task AddAsync([Remainder] string data)
        {
            await DoAsync(async () =>
            {
                var fields = data.Split("\n").Where(o => !string.IsNullOrEmpty(o)).ToArray();

                if (fields.Length < 3)
                    throw new ArgumentException("Nebyly zadány všechny potřebné parametry. (Musí obsahovat, Odpověď, Typ porovnání)");

                var paramsFlags = fields.Length > 3 ? Convert.ToInt32(fields[3]) : 0;
                var disabled = (paramsFlags & (int)AutoReplyParams.Disabled) != 0;
                var caseSensitive = (paramsFlags & (int)AutoReplyParams.CaseSensitive) != 0;

                await Service.AddReplyAsync(fields[0], fields[1], fields[2], disabled, caseSensitive).ConfigureAwait(false);
                await ReplyAsync($"Automatická odpověď **{fields[0]}** => **{fields[1]}** byla úspěšně přidána.").ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("edit")]
        [Summary("Modifikuje existující automatickou odpověď.")]
        [Remarks("Parametry jsou téměř stejné jako u metody Add, jen s tím rozdílem, že je jako první parametr očekáváno ID.")]
        public async Task Edit(int id, [Remainder] string data)
        {
            await DoAsync(async () =>
            {
                var fields = data.Split("\n").Where(o => !string.IsNullOrEmpty(o)).ToArray();

                if (fields.Length < 3)
                    throw new ArgumentException("Nebyly zadány všechny potřebné parametry.");

                var paramsFlags = fields.Length > 3 ? Convert.ToInt32(fields[3]) : 0;
                var caseSensitive = (paramsFlags & (int)AutoReplyParams.CaseSensitive) != 0;

                await Service.EditReplyAsync(id, fields[0], fields[1], fields[2], caseSensitive).ConfigureAwait(false);
                await ReplyAsync($"Automatická odpověď **{fields[0]}** => **{fields[1]}** byla úspěšně upravená.").ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("remove")]
        [Summary("Odebere automatickou odpověď.")]
        public async Task Remove(int id)
        {
            await DoAsync(async () =>
            {
                await Service.RemoveReplyAsync(id).ConfigureAwait(false);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně odebrána.").ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}