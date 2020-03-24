namespace Grillbot.Database.Entity.Views
{
    public class SummarizedCommandLog
    {
        public string Group { get; set; }
        public string Command { get; set; }
        public int Count { get; set; }
    }
}
