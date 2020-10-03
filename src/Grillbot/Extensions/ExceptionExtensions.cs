using System;
using System.Collections.Generic;

namespace Grillbot.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception ex)
        {
            var messages = new List<string>();

            var exception = ex;
            while (exception != null)
            {
                messages.Add(exception.Message);
                exception = exception.InnerException;
            }

            return string.Join(" --> ", messages);
        }
    }
}
