using Discord.Commands;
using Grillbot.Services.Preconditions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("autoreply")]
    [Name("Ovládání automatických odpovědí")]
    [RequirePermissions("AutoReply")]
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
            var data = Service.ListItems();

            if (data.Count > 0)
                await ReplyAsync(string.Join(Environment.NewLine, data));
            else
                await ReplyAsync("Ještě nejsou uloženy žádné odpověďi.");
        }

        [Command("disable")]
        [Summary("Deaktivuje automatickou odpověď.")]
        public async Task DisableAsync(int id)
        {
            await DoAsync(async () =>
            {
                await Service.SetActiveStatusAsync(id, true);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně deaktivována.");
            });
        }

        [Command("enable")]
        [Summary("Aktivuje automatickou odpověď.")]
        public async Task EnableAsync(int id)
        {
            await DoAsync(async () =>
            {
                await Service.SetActiveStatusAsync(id, false);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně aktivována.");
            });
        }

        [Command("add")]
        [Summary("Přidá novou automatickou odpověď.")]
        [Remarks("Parametry jsou odděleny novým řádkem, očekávaný jsou parametry {MustContains}\\n{ReplyMessage}\\nTyp porovnání (==, Contains)\\n[{IsDisabled}]")]
        public async Task AddAsync([Remainder] string data)
        {
            await DoAsync(async () =>
            {
                var fields = data.Split("\n").Where(o => !string.IsNullOrEmpty(o)).ToArray();

                if (fields.Length < 3)
                    throw new ArgumentException("Nebyly zadány všechny potřebné parametry. (Musí obsahovat, Odpověď, Typ porovnání)");

                await Service.AddReplyAsync(fields[0], fields[1], fields[2], fields.Length > 3 && Convert.ToBoolean(fields[3]));
                await ReplyAsync($"Automatická odpověď **{fields[0]}** => **{fields[1]}** byla úspěšně přidána.");
            });
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

                await Service.EditReplyAsync(id, fields[0], fields[1], fields[2]);
                await ReplyAsync($"Automatická odpověď **{fields[0]}** => **{fields[1]}** byla úspěšně upravená.");
            });
        }

        [Command("remove")]
        [Summary("Odebere automatickou odpověď.")]
        public async Task Remove(int id)
        {
            await DoAsync(async () =>
            {
                await Service.RemoveReplyAsync(id);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně odebrána.");
            });
        }
    }
}