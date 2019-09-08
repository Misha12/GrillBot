using Newtonsoft.Json;
using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBotMath
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            MathCalcResult result = null;
            var input = string.Join(" ", args);

            var expressionFields = input.Split(';').Select(o => o.Trim());
            var arguments = ParseArguments(expressionFields);
            var expressionData = expressionFields.FirstOrDefault(o => !IsVariableDeclaration(o));

            if(string.IsNullOrEmpty(expressionData))
            {
                Console.WriteLine(JsonConvert.SerializeObject(new MathCalcResult()
                {
                    ErrorMessage = "Nelze spočítat prázdný požadavek."
                }));

                return;
            }

            var expression = new Expression(expressionData, arguments.ToArray());
            var errorMessages = ValidateAndGetErrors(expression);

            if (errorMessages.Count > 0)
            {
                result = new MathCalcResult()
                {
                    ErrorMessage = string.Join(Environment.NewLine, errorMessages)
                };

                Console.WriteLine(JsonConvert.SerializeObject(result));
                return;
            }

            result = new MathCalcResult()
            {
                IsValid = true,
                Result = expression.calculate(),
                ComputingTime = expression.getComputingTime(),
            };

            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        public static List<string> ValidateAndGetErrors(Expression expression)
        {
            var errorMessages = new List<string>();

            if (!expression.checkLexSyntax())
                errorMessages.Add(expression.getErrorMessage());

            if (!expression.checkSyntax())
            {
                try
                {
                    if (!CheckMissingParameters(expression, out string errorMessage))
                        errorMessages.Add(errorMessage);
                    else if (!CheckMissingFunctions(expression, out errorMessage))
                        errorMessages.Add(errorMessage);
                    else
                        errorMessages.Add("Syntax error");
                }
                catch(Exception ex)
                {
                    errorMessages.Add(ex.Message);
                }
            }

            return errorMessages;
        }

        private static bool MissingDataCheck(Expression expression, Func<Expression, string[]> func, string errorMessageTemplate, out string errorMessage)
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

        private static bool CheckMissingParameters(Expression expression, out string errorMessage)
        {
            return MissingDataCheck(expression, e => e.getMissingUserDefinedArguments(), "Chybí mi parametr: {0}", out errorMessage);
        }

        private static bool CheckMissingFunctions(Expression expression, out string errorMessage)
        {
            return MissingDataCheck(expression, o => o.getMissingUserDefinedFunctions(), "Chybí mi funkce: {0}", out errorMessage);
        }

        private static bool IsVariableDeclaration(string field) => field.Trim().StartsWith("var", StringComparison.InvariantCultureIgnoreCase);

        private static List<Argument> ParseArguments(IEnumerable<string> expressionFields)
        {
            return expressionFields
                .Where(IsVariableDeclaration)
                .Select(o => o.Substring(4).Split('='))
                .Select(o => new Argument(o[0].Trim(), Convert.ToDouble(o[1].Trim())))
                .ToList();
        }
    }
}
