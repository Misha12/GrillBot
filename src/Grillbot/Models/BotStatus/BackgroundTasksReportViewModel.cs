using System.Collections.Generic;

namespace Grillbot.Models.BotStatus
{
    public class BackgroundTasksReportViewModel
    {
        public Dictionary<string, List<string>> Groups { get; set; }

        public BackgroundTasksReportViewModel(Dictionary<string, List<string>> groups)
        {
            Groups = groups;
        }
    }
}
