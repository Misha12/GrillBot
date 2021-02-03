using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Models.Embed;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Models.Math;
using Grillbot.Services;
using Grillbot.Services.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Počítání")]
    [ModuleID("MathModule")]
    [Group("math")]
    public class MathModule : BotModuleBase
    {
        private MathService Calculator { get; }

        public MathModule(MathService calculator, PaginationService pagination) : base(paginationService: pagination)
        {
            Calculator = calculator;
        }

        [Command("solve")]
        [Summary("Matematické výpočty")]
        [Remarks("Příklad běžného použití: **solve 1+1**\nPříklad použití s konstanty: **const x = 5; const y = 10; rand(x, y)**\nExtra funkce:\n- rand(x, y): Náhodné" +
            " číslo na intervalu <x; y>\n- Fib(x): Fibonacciho posloupnost.")]
        public async Task SolveAsync([Remainder] string expression)
        {
            var result = Calculator.Solve(expression, Context.Message);

            var embed = new BotEmbed(Context.Message.Author, Color.Green)
                .AddField("Výraz", $"`{expression}`", false);

            if (result == null)
            {
                embed
                    .SetColor(Color.Red)
                    .WithTitle("Při zpracování výrazu došlo k neznámé chybě.");

                await ReplyAsync(embed: embed.Build());
                return;
            }

            if (!result.IsValid)
            {
                embed.SetColor(Color.Red);

                if (result.IsTimeout)
                {
                    embed
                        .WithTitle("Vypršel časový limit pro výpočet výrazu.")
                        .AddField("Maximální doba zpracování", result.GetAssignedComputingTime(), false);
                }
                else
                {
                    embed
                        .WithTitle("Výpočet nebyl úspěšně proveden.")
                        .AddField("Chybové hlášení", result.ErrorMessage.Trim(), false);
                }

                await ReplyAsync(embed: embed.Build());
                return;
            }

            embed
                .AddField("Výsledek", result.Result.ToString(), true)
                .AddField("Doba zpracování", result.GetComputingTime(), true);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("sessions")]
        [Summary("Výpočetní sessions")]
        public async Task SessionsListAsync()
        {
            var boosterSessions = Calculator.Sessions.Where(o => o.ForBooster);
            var otherSessions = Calculator.Sessions.Where(o => !o.ForBooster);

            var embed = new PaginatedEmbed()
            {
                Pages = new List<PaginatedEmbedPage>()
                {
                    RenderPage(boosterSessions, true),
                    RenderPage(otherSessions, false)
                },
                ResponseFor = Context.User,
                Title = "Výpočetní sessions"
            };

            await SendPaginatedEmbedAsync(embed);
        }

        private static PaginatedEmbedPage RenderPage(IEnumerable<MathSession> sessions, bool booster)
        {
            var page = new PaginatedEmbedPage(booster ? "Server booster" : "Ostatní sessions");

            foreach (var session in sessions)
            {
                page.AddField(
                    $"#{session.ID} {(session.IsUsed ? "(Používá se)" : "")}".Trim(),
                    $"Čas: **{TimeSpan.FromMilliseconds(session.ComputingTime)}**\nPočet použití: **{session.UsedCount.FormatWithSpaces()}**",
                    true
                );
            }

            return page;
        }

        [Command("session")]
        [Summary("Detail výpočetní jednotky.")]
        public async Task SessionDetailAsync(int sessionId)
        {
            var session = Calculator.Sessions.Find(o => o.ID == sessionId);

            if (session == null)
            {
                await ReplyAsync($"Session s ID {sessionId} nebyla nalezena.");
                return;
            }

            var color = session.IsUsed ? Color.Green : Color.LightGrey;
            var title = $"#{session.ID} {(session.IsUsed ? "(Používá se)" : "")}";

            if (session.ForBooster)
                title += " (Booster)";

            var embed = new BotEmbed(Context.User, color, title.Trim())
                .AddField("Výpočetní čas", TimeSpan.FromMilliseconds(session.ComputingTime).ToString(), true)
                .AddField("Počet použití", session.UsedCount.FormatWithSpaces(), true);

            embed
                .AddField("Aktuální výraz", session.Expression ?? "-", false)
                .AddField("Poslední výsledek", session.LastResult?.Format() ?? "-", false);

            await ReplyAsync(embed: embed.Build());
        }
    }
}
