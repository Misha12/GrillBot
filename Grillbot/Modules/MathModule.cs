using Discord.Commands;
using Grillbot.Services;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Počítání")]
    public class MathModule : BotModuleBase
    {
        private MathCalculator Calculator { get; }

        public MathModule(MathCalculator calculator)
        {
            Calculator = calculator;
        }

        [Command("solve")]
        [RequireRoleOrAdmin(RoleGroupName = "Math")]
        [DisabledCheck(RoleGroupName = "Math")]
        public async Task Solve(params string[] expressions)
        {
            var expressionData = string.Join(" ", expressions);
            var result = Calculator.Solve(expressionData, Context.Message);

            if(!result.IsValid)
            {
                await ReplyAsync($"{result.Mention} {result.ErrorMessage}");
                return;
            }

            await ReplyAsync($"{result.Mention} Výsledek je: {result.Result.ToString()}");
        }
    }
}
