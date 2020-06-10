using Newtonsoft.Json;
using System;

namespace GrillBotMath
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            MathCalcResult result;
            var parser = new ExpressionParser(string.Join(" ", args));

            if (parser.Empty)
            {
                Console.WriteLine(JsonConvert.SerializeObject(new MathCalcResult()
                {
                    ErrorMessage = "Nelze spočítat prázdný požadavek."
                }));

                return;
            }

            if (!parser.IsValid)
            {
                result = new MathCalcResult()
                {
                    ErrorMessage = string.Join(Environment.NewLine, parser.Errors)
                };

                Console.WriteLine(JsonConvert.SerializeObject(result));
                return;
            }

            result = new MathCalcResult()
            {
                IsValid = true,
                Result = parser.Expression.calculate(),
                ComputingTime = parser.Expression.getComputingTime() * 1000.0,
            };

            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}
