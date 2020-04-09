using Grillbot.Models.InMemoryLogger;
using Logging.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.InMemoryLogger
{
    public class InMemoryLoggerService
    {
        public List<LogEntry> GetLogEntries(LogLevel minLevel, string section)
        {
            var query = MemoryLogger.GetLogGte(minLevel)
                .Select(JsonConvert.DeserializeObject<LogEntry>);

            if (!string.IsNullOrEmpty(section))
                query = query.Where(o => o.LogName.StartsWith(section, StringComparison.InvariantCultureIgnoreCase));

            return query.OrderByDescending(o => o.ID).ToList();
        }
    }
}
