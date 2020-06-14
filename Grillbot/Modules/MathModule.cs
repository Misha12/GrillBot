using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Models.Embed;
using Grillbot.Services.Math;
using Grillbot.Services.Permissions.Preconditions;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Počítání")]
    [RequirePermissions]
    [ModuleID("MathModule")]
    public class MathModule : BotModuleBase
    {
        private MathService Calculator { get; }

        public MathModule(MathService calculator)
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
    }
}
