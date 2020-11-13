using Grillbot.Database.Entity;
using System.Collections.Generic;

namespace Grillbot.Models.ErrorLog
{
    public class ErrorLogViewModel
    {
        public long? ID { get; set; }
        public ErrorLogItem LogItem { get; set; }
        public bool? Found { get; set; }
        public List<ErrorLogItem> Logs { get; set; }

        public ErrorLogViewModel(ErrorLogItem item, List<ErrorLogItem> logs, bool haveId)
        {
            LogItem = item;
            ID = item?.ID;
            Found = !haveId ? null : item != null;
            Logs = logs;
        }
    }
}
