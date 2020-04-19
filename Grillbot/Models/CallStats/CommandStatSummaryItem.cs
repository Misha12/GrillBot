namespace Grillbot.Models.CallStats
{
    public class CommandStatSummaryItem
    {
        public string Group { get; set; }
        public string Command { get; set; }
        public long CallsCount { get; set; }
        public string Guild { get; set; }
        public int PermissionsCount { get; set; }
    }
}
