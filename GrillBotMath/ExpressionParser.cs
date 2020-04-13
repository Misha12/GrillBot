using GrillBotMath.Functions;
using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBotMath
{
    public class ExpressionParser
    {
        public Expression Expression { get; }
        public List<string> Errors { get; }

        public bool Empty => Expression == null;
        public bool IsValid => Errors.Count == 0;

        public ExpressionParser(string data)
        {
            Errors = new List<string>();

            var expressionFields = data.Split(';').Select(o => o.Trim());
            var arguments = ParseArguments(expressionFields);
            var expressionData = expressionFields.FirstOrDefault(o => !IsVariableDeclaration(o));

            if (string.IsNullOrEmpty(expressionData)) return;

            foreach (var func in FunctionsList.Functions)
            {
                expressionData = func.FixExpression(expressionData);
            }

            Expression = new Expression(expressionData);

            Expression.addArguments(arguments.ToArray());
            Expression.addFunctions(FunctionsList.Functions.Select(func => new Function(func.FunctionName, func)).ToArray());

            Errors.AddRange(ValidateAndGetErrors(Expression));
        }

        private List<Argument> ParseArguments(IEnumerable<string> arguments)
        {
            return arguments
                .Where(IsVariableDeclaration)
                .Select(o => o.Substring(4).Split('='))
                .Select(o => new Argument(o[0].Trim(), Convert.ToDouble(o[1].Trim())))
                .ToList();
        }

        private bool IsVariableDeclaration(string field) => field.Trim().StartsWith("var", StringComparison.InvariantCultureIgnoreCase);

        private List<string> ValidateAndGetErrors(Expression expression)
        {
            var errorMessages = new List<string>();

            if (!expression.checkSyntax())
            {
                try
                {
                    if (!CheckMissingParameters(expression, out string errorMessage))
                        errorMessages.Add(errorMessage);

                    if (!CheckMissingFunctions(expression, out errorMessage))
                        errorMessages.Add(errorMessage);

                    errorMessages.Add("Syntax error");
                }
                catch (Exception ex)
                {
                    errorMessages.Add(ex.Message);
                }
            }

            return errorMessages;
        }

        private bool MissingDataCheck(Expression expression, Func<Expression, string[]> func, string errorMessageTemplate, out string errorMessage)
        {
            var missing = func(expression);

            if (missing.Length > 0)
            {
                errorMessage = string.Format(errorMessageTemplate, string.Join(", ", missing));
                return false;
            }

            errorMessage = null;
            return true;
        }

        private bool CheckMissingParameters(Expression expression, out string errorMessage)
        {
            return MissingDataCheck(expression, e => e.getMissingUserDefinedArguments(), "Chybí mi parametr: {0}", out errorMessage);
        }

        private bool CheckMissingFunctions(Expression expression, out string errorMessage)
        {
            return MissingDataCheck(expression, o => o.getMissingUserDefinedFunctions(), "Chybí mi funkce: {0}", out errorMessage);
        }
    }
}
