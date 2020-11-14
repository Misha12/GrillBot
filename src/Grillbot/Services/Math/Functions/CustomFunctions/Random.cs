using org.mariuszgromada.math.mxparser;
using System;

namespace Grillbot.Services.Math.Functions.CustomFunctions
{
    public class Random : ICustomFunction
    {
        public string FunctionName => "rand";

        public double A { get; set; }
        public double B { get; set; }

        public Random() : this(double.MinValue, double.MinValue) { }

        public Random(double a, double b)
        {
            A = a;
            B = b;
        }

        public double calculate()
        {
            // Thanks stack overflow.
            /// <see cref="https://stackoverflow.com/a/50953366"/>
            var random = new System.Random();
            var next = random.NextDouble();

            return A + (next * (B - A));
        }

        public FunctionExtension clone()
        {
            return new Random(A, B);
        }

        public string getParameterName(int parameterIndex)
        {
            return parameterIndex switch
            {
                0 => "A",
                1 => "B",
                _ => null,
            };
        }

        public int getParametersNumber()
        {
            return 2;
        }

        public void setParameterValue(int parameterIndex, double parameterValue)
        {
            var floor = System.Math.Floor(parameterValue);

            switch (parameterIndex)
            {
                case 0:
                    A = floor;
                    break;
                case 1:
                    B = floor;
                    break;
            }
        }
    }
}
