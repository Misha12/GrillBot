using System.Collections.Generic;

namespace Grillbot.Database.Entity.Views
{
    public class SummarizedCommandLog
    {
        public string Group { get; set; }
        public string Command { get; set; }
        public int Count { get; set; }
        public Dictionary<int, string> Methods { get; set; }
        public int PermissionsCount { get; set; }
    }
}
