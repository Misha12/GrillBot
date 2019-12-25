using Discord;
using Discord.Commands;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Math;
using Grillbot.Services.Preconditions;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Počítání")]
    [RequirePermissions("Math", BoosterAllowed = true)]
    public class MathModule : BotModuleBase
    {
        private MathService Calculator { get; }

        public MathModule(MathService calculator)
        {
            Calculator = calculator;
        }

        [Command("solve")]
        public async Task SolveAsync([Remainder] string expression)
        {
            var result = Calculator.Solve(expression, Context.Message);

            var embed = new EmbedBuilder()
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro {Context.Message.Author.GetFullName()}", Context.Message.Author.GetUserAvatarUrl());

            if(!result.IsValid)
            {
                embed
                    .WithColor(Color.Red)
                    .WithTitle("Výpočet nebyl úspěšně proveden.")
                    .WithFields(
                        new EmbedFieldBuilder().WithName("Výraz").WithValue($"`{expression}`"),
                        new EmbedFieldBuilder().WithName("Maximální doba zpracování").WithValue(result.GetAssignedComputingTime()),
                        new EmbedFieldBuilder().WithName("Chybové hlášení").WithValue(result.ErrorMessage.Trim())
                    );

                await ReplyAsync(result.GetMention(), embed: embed.Build()).ConfigureAwait(false);
                return;
            }

            embed
                .WithColor(Color.Green)
                .WithTitle("Výpočet byl úspěšně dokončen.")
                .WithFields(
                    new EmbedFieldBuilder().WithName("Výraz").WithValue($"`{expression}`"),
                    new EmbedFieldBuilder().WithName("Výsledek").WithValue(result.Result.ToString()),
                    new EmbedFieldBuilder().WithName("Doba zpracování").WithValue(result.GetComputingTime()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Maximální doba zpracování").WithValue(result.GetAssignedComputingTime()).WithIsInline(true)
                );

            await ReplyAsync(result.GetMention(), embed: embed.Build()).ConfigureAwait(false);
        }
    }
}
