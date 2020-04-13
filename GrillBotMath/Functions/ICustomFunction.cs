using org.mariuszgromada.math.mxparser;

namespace GrillBotMath.Functions
{
    public interface ICustomFunction : FunctionExtension
    {
        string FunctionName { get; }
        string FixExpression(string expression) { return expression; }
    }
}
