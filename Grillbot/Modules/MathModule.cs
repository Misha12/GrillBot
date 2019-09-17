using Discord.Commands;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Počítání")]
    [RequirePermissions("Math")]
    public class MathModule : BotModuleBase
    {
        private MathCalculator Calculator { get; }

        public MathModule(MathCalculator calculator)
        {
            Calculator = calculator;
        }

        [Command("solve")]
        public async Task SolveAsync([Remainder] string expression)
        {
            var result = Calculator.Solve(expression, Context.Message);

            if(!result.IsValid)
            {
                await ReplyAsync($"{result.GetMention()} {result.ErrorMessage}".Trim());
                return;
            }

            await ReplyAsync($"{result.GetMention()} Výsledek je: {result.Result.ToString()}, doba zpracování byla {result.ComputingTime} ms".Trim());
        }
    }
}
