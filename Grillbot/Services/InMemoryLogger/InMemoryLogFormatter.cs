using Grillbot.Models.InMemoryLogger;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace Grillbot.Services.InMemoryLogger
{
    public static class InMemoryLogFormatter
    {
        private static long Counter = 0;

        public static string Format(LogLevel level, string logName, string message, Exception exception)
        {
            var id = Interlocked.Increment(ref Counter);

            var entry = new LogEntry()
            {
                DateTime = DateTime.Now,
                Exception = exception,
                Level = level,
                LogName = logName,
                Message = message,
                ID = id
            };

            return JsonConvert.SerializeObject(entry);
        }
    }
}
