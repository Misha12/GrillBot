using org.mariuszgromada.math.mxparser;

namespace Grillbot.Services.Math.Functions
{
    public interface ICustomFunction : FunctionExtension
    {
        string FunctionName { get; }
        string FixExpression(string expression) { return expression; }
    }
}
