using Grillbot.Database.Entity;
using Grillbot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.AutoReply
{
    public class AutoReplyItem
    {
        public int ID { get; set; }
        public string MustContains { get; set; }
        public string Reply { get; set; }
        public AutoReplyCompareTypes CompareType { get; set; }
        public string Channel { get; set; }
        public int Flags { get; set; }

        public IEnumerable<string> GetFlagValues()
        {
            return Enum.GetValues<AutoReplyParams>()
                .Where(o => (int)o > 0 && (Flags & (int)o) != 0)
                .Select(o => o.ToString());
        }
    }
}
