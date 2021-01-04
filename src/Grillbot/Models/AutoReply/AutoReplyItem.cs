using Grillbot.Database.Entity;

namespace Grillbot.Models.AutoReply
{
    public class AutoReplyItem
    {
        public int ID { get; set; }
        public string MustContains { get; set; }
        public string Reply { get; set; }
        public bool IsActive { get; set; }
        public AutoReplyCompareTypes CompareType { get; set; }
        public int CallsCount { get; set; }
        public bool CaseSensitive { get; set; }
        public string Channel { get; set; }
    }
}
