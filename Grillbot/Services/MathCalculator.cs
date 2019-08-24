using Discord.WebSocket;
using Grillbot.Models;
using Microsoft.Extensions.Configuration;
using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class MathCalculator
    {
        private IConfiguration Config { get; }

        public MathCalculator(IConfiguration config)
        {
            Config = config;
        }

        public MathCalcResult Solve(string input, SocketUserMessage message)
        {
            var expressionFields = input.Split(';').Select(o => o.Trim());
            var arguments = ParseArguments(expressionFields);
            var expressionData = expressionFields.FirstOrDefault(o => !IsVariableDeclaration(o));

            if (string.IsNullOrEmpty(expressionData))
                return new MathCalcResult(message?.Author.Mention, "Nelze spočítat prázdný požadavek.");

            if (expressionData.Contains("nan", StringComparison.InvariantCultureIgnoreCase))
                return new MathCalcResult(message?.Author.Mention, "Toho bys asi chtěl moc.");

            var expression = new Expression(expressionData, arguments.ToArray());
            var errorMessages = ValidateAndGetErrors(expression);

            if (errorMessages.Count > 0)
                return new MathCalcResult(message.Author.Mention, string.Join(Environment.NewLine, errorMessages));

            var calcTask = Task.Run(() =>
            {
                var result = expression.calculate();
                var time = expression.getComputingTime();
                return new Tuple<double, double>(result, time);
            });
            var calcTime = Convert.ToInt32(Config["MethodsConfig:Math:ComputingTime"]);

            if (!calcTask.Wait(calcTime))
                return new MathCalcResult(message.Author.Mention, "Vypršel mi časový limit na výpočet příkladu.");

            var calcTaskResult = calcTask.Result;
            return new MathCalcResult(message.Author.Mention, calcTaskResult.Item1, calcTaskResult.Item2);
        }

        public List<string> ValidateAndGetErrors(Expression expression)
        {
            var errorMessages = new List<string>();

            if (!expression.checkLexSyntax() || !expression.checkSyntax())
            {
                if (!CheckMissingParameters(expression, out string errorMessage))
                    errorMessages.Add(errorMessage);
                else if (!CheckMissingFunctions(expression, out errorMessage))
                    errorMessages.Add(errorMessage);
                else
                    errorMessages.Add("Syntax error");
            }

            return errorMessages;
        }

        private bool MissingDataCheck(Expression expression, Func<Expression, string[]> func, string errorMessageTemplate, out string errorMessage)
        {
            var missing = func(expression);

            if(missing.Length > 0)
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

        private bool IsVariableDeclaration(string field) => field.Trim().StartsWith("var", StringComparison.InvariantCultureIgnoreCase);

        private List<Argument> ParseArguments(IEnumerable<string> expressionFields)
        {
            return expressionFields
                .Where(IsVariableDeclaration)
                .Select(o => o.Substring(4).Split('='))
                .Select(o => new Argument(o[0].Trim(), Convert.ToDouble(o[1].Trim())))
                .ToList();
        }
    }
}
