using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Enums;
using Grillbot.Extensions;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Modules.AutoReply;
using Grillbot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("autoreply")]
    [ModuleID(nameof(AutoReplyModule))]
    [Name("Ovládání automatických odpovědí")]
    public class AutoReplyModule : BotModuleBase
    {
        public AutoReplyModule(PaginationService paginationService, IServiceProvider provider) : base(paginationService: paginationService, provider: provider)
        {
        }

        [Command("list")]
        [Summary("Vypíše všechny možné odpovědi.")]
        public async Task ListItemsAsync()
        {
            using var service = GetService<AutoReplyService>();
            var data = service.Service.GetList(Context.Guild);

            if (data.Count == 0)
            {
                await ReplyAsync("Ještě nejsou uloženy žádné odpovědi.");
                return;
            }

            var pages = new List<PaginatedEmbedPage>();
            foreach (var item in data)
            {
                var page = new PaginatedEmbedPage($"**{item.ID} - {item.MustContains}**");

                var builder = new EmbedFieldBuilder()
                    .WithName($"Odpověď: {item.Reply}")
                    .WithValue(string.Join("\n", new[] {
                        $"Status: {(item.IsActive ? "Aktivní" : "Neaktivní")}",
                        $"Metoda: {item.CompareType}",
                        $"Počet použití: {item.CallsCount.FormatWithSpaces()}",
                        $"Case sensitive: {(item.CaseSensitive ? "Ano" : "Ne")}",
                        $"Kanál: {item.Channel}"
                    }));

                page.AddField(builder);
                pages.Add(page);
            }

            var embed = new PaginatedEmbed()
            {
                Title = "Automatické odpovědi",
                ResponseFor = Context.User,
                Pages = pages
            };

            await SendPaginatedEmbedAsync(embed);
        }

        [Command("disable")]
        [Summary("Deaktivuje automatickou odpověď.")]
        public async Task DisableAsync(int id)
        {
            try
            {
                using var service = GetService<AutoReplyService>();
                await service.Service.SetActiveStatusAsync(Context.Guild, id, true);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně deaktivována.");
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("enable")]
        [Summary("Aktivuje automatickou odpověď.")]
        public async Task EnableAsync(int id)
        {
            try
            {
                using var service = GetService<AutoReplyService>();
                await service.Service.SetActiveStatusAsync(Context.Guild, id, false).ConfigureAwait(false);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně aktivována.").ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("add")]
        [Summary("Přidá novou automatickou odpověď.")]
        [Remarks("Parametry jsou odděleny novým řádkem, očekávaný jsou parametry {MustContains}\\n{ReplyMessage}\\nTyp porovnání (==, Contains)\\n{Příznaky}\n{ID Kanalu}\n" +
            "Příznaky: 1. bit: CaseSensitive, 2. bit: Deaktivovat.\nPokud se má odpovídat všude, tak se očekává \\*." +
            "\n\nŠablona:\n{MustContains}\n{ReplyMessage}\n{==/Contains}\n0\n{ChannelID/\\*}")]
        public async Task AddAsync([Remainder] string data)
        {
            try
            {
                var fields = data.Split("\n").Where(o => !string.IsNullOrEmpty(o)).ToArray();

                if (fields.Length < 5)
                {
                    await ReplyAsync("Nebyly zadány všechny potřebné parametry. (Musí obsahovat, Odpověď, Typ porovnání, Příznaky, Kanál)");
                    return;
                }

                var paramsFlags = Convert.ToInt32(fields[3]);
                var disabled = (paramsFlags & (int)AutoReplyParams.Disabled) != 0;
                var caseSensitive = (paramsFlags & (int)AutoReplyParams.CaseSensitive) != 0;

                using var service = GetService<AutoReplyService>();
                await service.Service.AddReplyAsync(Context.Guild, fields[0], fields[1], fields[2], disabled, caseSensitive, fields[4]);
                await ReplyAsync($"Automatická odpověď **{fields[0]}** => **{fields[1]}** byla úspěšně přidána.").ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("edit")]
        [Summary("Modifikuje existující automatickou odpověď.")]
        [Remarks("Parametry jsou téměř stejné jako u metody Add, jen s tím rozdílem, že je jako první parametr očekáváno ID.")]
        public async Task EditAsync(int id, [Remainder] string data)
        {
            try
            {
                var fields = data.Split("\n").Where(o => !string.IsNullOrEmpty(o)).ToArray();

                if (fields.Length < 3)
                {
                    await ReplyAsync("Nebyly zadány všechny potřebné parametry.");
                    return;
                }

                var paramsFlags = fields.Length > 3 ? Convert.ToInt32(fields[3]) : 0;
                var caseSensitive = (paramsFlags & (int)AutoReplyParams.CaseSensitive) != 0;

                using var service = GetService<AutoReplyService>();
                await service.Service.EditReplyAsync(Context.Guild, id, fields[0], fields[1], fields[2], caseSensitive, fields[4]);
                await ReplyAsync($"Automatická odpověď **{fields[0]}** => **{fields[1]}** byla úspěšně upravená.").ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("remove")]
        [Summary("Odebere automatickou odpověď.")]
        public async Task RemoveAsync(int id)
        {
            try
            {
                using var service = GetService<AutoReplyService>();
                await service.Service.RemoveReplyAsync(Context.Guild, id).ConfigureAwait(false);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně odebrána.").ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }
    }
}