using Microsoft.Extensions.Logging;
using System;

namespace Grillbot.Models.InMemoryLogger
{
    public class LogEntry
    {
        public long ID { get; set; }
        public DateTime DateTime { get; set; }
        public LogLevel Level { get; set; }
        public string LogName { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
