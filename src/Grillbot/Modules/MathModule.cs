using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Models.Embed;
using Grillbot.Models.Math;
using Grillbot.Services.Math;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Počítání")]
    [ModuleID(nameof(MathModule))]
    [Group("math")]
    public class MathModule : BotModuleBase
    {
        private ILogger<MathModule> Logger { get; }
        private IConfiguration Configuration { get; }

        public MathModule(ILogger<MathModule> logger, IConfiguration configuration)
        {
            Logger = logger;
            Configuration = configuration;
        }

        [Command("solve")]
        [Summary("Matematické výpočty")]
        [Remarks("Příklad běžného použití: **solve 1+1**\nPříklad použití s konstanty: **const x = 5; const y = 10; rand(x, y)**\nExtra funkce:\n- rand(x, y): Náhodné" +
            " číslo na intervalu <x; y>\n- Fib(x): Fibonacciho posloupnost.")]
        public async Task SolveAsync([Remainder] string expression)
        {
            var result = Solve(expression);

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

        private MathCalcResult Solve(string input)
        {
            var boosterRoleId = Configuration["ServerBoosterRoleId"];
            bool booster = !string.IsNullOrEmpty(boosterRoleId) && ((Context.User as SocketGuildUser)?.Roles?.Any(o => o.Id == Convert.ToUInt64(boosterRoleId)) ?? false);
            var time = 10000 * (booster ? 3 : 1);
            input = ("" + input).Trim(); // treatment against null values.

            var parser = new ExpressionParser(input);

            if (parser.Empty)
                return new MathCalcResult() { ErrorMessage = "Nelze spočítat prázdný výraz." };

            if (!parser.IsValid)
                return new MathCalcResult() { ErrorMessage = string.Join(Environment.NewLine, parser.Errors) };

            try
            {
                var task = Task.Run(() =>
                {
                    return new MathCalcResult()
                    {
                        IsValid = true,
                        Result = parser.Expression.calculate(),
                        ComputingTime = parser.Expression.getComputingTime() * 1000
                    };
                });

                if (!task.Wait(time))
                {
                    try
                    {
                        task.Dispose();
                    }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                    catch (Exception) { /* This exception we can ignore. */ }
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.

                    return new MathCalcResult()
                    {
                        IsTimeout = true,
                        AssingedComputingTime = time
                    };
                }
                else
                {
                    return task.Result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");

                return new MathCalcResult()
                {
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
