using GrillBotMath.Functions;

namespace GrillBotMath
{
    public static class TransformExpression
    {
        public static string ProcessExpression(string expression)
        {
            expression = ProcessModulo(expression);
            expression = ProcessCustomFunctions(expression);

            return expression;
        }

        private static string ProcessModulo(string expression)
        {
            if (!expression.Contains("%"))
                return expression;

            return expression.Replace("%", "#");
        }

        private static string ProcessCustomFunctions(string expression)
        {
            foreach (var function in FunctionsList.GetCustomFunctions())
            {
                expression = function.FixExpression(expression);
            }

            return expression;
        }
    }
}
