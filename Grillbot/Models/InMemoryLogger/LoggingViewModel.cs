using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Grillbot.Models.InMemoryLogger
{
    public class LoggingViewModel
    {
        public LogLevel SelectedLevel { get; set; }
        public string Section { get; set; }
        public List<LogEntry> Entries { get; set; }

        public LoggingViewModel(List<LogEntry> entries, LogLevel selectedLevel, string section)
        {
            Entries = entries ?? new List<LogEntry>();
            SelectedLevel = selectedLevel;
            Section = section;
        }
    }
}
