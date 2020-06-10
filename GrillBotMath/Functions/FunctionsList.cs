using GrillBotMath.Functions.CustomFunctions;
using System.Collections.Generic;

namespace GrillBotMath.Functions
{
    public static class FunctionsList
    {
        private static ICustomFunction[] Functions { get; set; }

        public static ICustomFunction[] GetCustomFunctions()
        {
            if(Functions == null)
            {
                Functions = new List<ICustomFunction>()
                {
                    new Fibonacci()
                }.ToArray();
            }

            return Functions;
        }
    }
}
