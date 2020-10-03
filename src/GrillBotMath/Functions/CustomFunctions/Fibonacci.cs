using org.mariuszgromada.math.mxparser;
using System;

namespace GrillBotMath.Functions.CustomFunctions
{
    public class Fibonacci : ICustomFunction
    {
        public string FunctionName => "Fib2";
        public double Input { get; set; }

        public Fibonacci() : this(double.NaN) { }

        public Fibonacci(double input)
        {
            Input = input;
        }

        public double calculate()
        {
            if (double.IsNaN(Input))
                return double.NaN;

            int n = (int)Math.Round(Input);
            if (n <= 1) return n;

            double minus2 = 0;
            double minus1 = 1;
            double fibo = minus1 + minus2;

            for (int i = 3; i <= n; i++)
            {
                minus2 = minus1;
                minus1 = fibo;
                fibo = minus1 + minus2;
            }

            return fibo;
        }

        public FunctionExtension clone()
        {
            return new Fibonacci(Input);
        }

        public string getParameterName(int parameterIndex)
        {
            return parameterIndex == 0 ? "N" : null;
        }

        public int getParametersNumber()
        {
            return 1;
        }

        public void setParameterValue(int parameterIndex, double parameterValue)
        {
            if (parameterIndex == 0)
                Input = parameterValue;
        }

        public string FixExpression(string expression)
        {
            return expression.Replace("Fib(", "Fib2(", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
