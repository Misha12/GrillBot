using org.mariuszgromada.math.mxparser;
using System;

namespace GrillBotMath.Functions.CustomFunctions
{
    public class Random : ICustomFunction
    {
        public string FunctionName => "rand";

        public int A { get; set; }
        public int B { get; set; }

        public Random() : this(int.MinValue, int.MinValue) { }

        public Random(int a, int b)
        {
            A = a;
            B = b;
        }

        public double calculate()
        {
            var random = new System.Random();
            return random.Next(A, B);
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
            switch(parameterIndex)
            {
                case 0:
                    A = Convert.ToInt32(Math.Floor(parameterValue));
                    break;
                case 1:
                    B = Convert.ToInt32(Math.Floor(parameterValue));
                    break;
            }
        }
    }
}
