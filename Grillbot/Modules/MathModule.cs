using Discord.Commands;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Počítání")]
    [RequireRoleOrAdmin(RoleGroupName = "Math")]
    [DisabledCheck(RoleGroupName = "Math")]
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
                await ReplyAsync($"{result.Mention} {result.ErrorMessage}");
                return;
            }

            await ReplyAsync($"{result.Mention} Výsledek je: {result.Result.ToString()}, doba zpracování byla {result.ComputingTime} ms");
        }
    }
}
