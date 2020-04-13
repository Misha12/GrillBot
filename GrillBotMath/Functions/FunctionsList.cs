using GrillBotMath.Functions.CustomFunctions;

namespace GrillBotMath.Functions
{
    public static class FunctionsList
    {
        public static ICustomFunction[] Functions { get; } = new[]
        {
            new Fibonacci()
        };
    }
}
