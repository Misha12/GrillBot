using Grillbot.Services.Math.Functions.CustomFunctions;
using System.Collections.Generic;

namespace Grillbot.Services.Math.Functions
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
                    new Fibonacci(),
                    new Random()
                }.ToArray();
            }

            return Functions;
        }
    }
}
