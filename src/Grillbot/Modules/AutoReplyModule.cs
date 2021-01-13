using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Models.AutoReply;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Modules.AutoReply;
using Grillbot.Services;
using System;
using System.Collections.Generic;
using System.Text;
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
                var page = new PaginatedEmbedPage(null);

                var builder = new EmbedFieldBuilder()
                    .WithName("Obecné")
                    .WithValue(string.Join("\n", new[] {
                        $"ID: {item.ID}",
                        $"Příznaky: {string.Join(", ", item.GetFlagValues())}",
                        $"Metoda: {item.CompareType}",
                        $"Počet použití: {item.CallsCount.FormatWithSpaces()}",
                        $"Kanál: {item.Channel}"
                    }));

                page.AddField(builder);
                page.AddField("Musí obsahovat", $"```{item.MustContains}```");
                page.AddField("Odpověď", $"```{item.Reply}```");

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
                var parsedData = AutoreplyData.Parse(data);

                using var service = GetService<AutoReplyService>();
                await service.Service.AddReplyAsync(Context.Guild, parsedData);
                await ReplyAsync("Automatická odpověď byla úspěšně přidána.");
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
                var parsedData = AutoreplyData.Parse(data);

                using var service = GetService<AutoReplyService>();
                await service.Service.EditReplyAsync(Context.Guild, id, parsedData);
                await ReplyAsync($"Automatická odpověď s ID **{id}** byla úspěšně upravená.");
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

        [Command("example")]
        [Summary("Vypíše příklad konfigurace pro nastavení automatické odpovědi.")]
        public async Task ShowExampleAsync()
        {
            var builder = new StringBuilder()
                .AppendLine("Parametry jsou odděleny novým řádkem, očekávaný jsou parametry {MustContains}\\n{ReplyMessage}\\nTyp porovnání (==, Contains)\\n{Příznaky}\n{ID Kanálu}. Parametry MustContains a ReplyMessage musí být v bloku kódu.")
                .AppendLine("Příznaky: 1. bit: CaseSensitive, 2. bit: Deaktivovat, 3. bit: Vypisovat odpovědi do bloku kódu.\nPokud se má odpovídat všude, tak se očekává \\*, jinak ID kanálu.")
                .AppendLine(new string('-', 50))
                .AppendLine("Šablona: ")
                .AppendLine("```MustContains``````ReplyMessage```{==/Contains}\n0\n{ChannelId/\\*}")
                .AppendLine(new string('-', 50));

            builder
                .AppendLine("RAW šablona:")
                .AppendLine("\\`\\`\\`")
                .AppendLine("MustContains")
                .AppendLine("\\`\\`\\`")
                .AppendLine("\\`\\`\\`")
                .AppendLine("ReplyMessage")
                .AppendLine("\\`\\`\\`")
                .AppendLine("==/Contains")
                .AppendLine("0")
                .AppendLine("*/ChannelID");

            await ReplyAsync(builder.ToString());
        }
    }
}