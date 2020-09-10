using Grillbot.Database.Entity;

namespace Grillbot.Models.ErrorLog
{
    public class ErrorLogViewModel
    {
        public long? ID { get; set; }
        public ErrorLogItem LogItem { get; set; }
        public bool? Found { get; set; }
    }
}
