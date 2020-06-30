namespace Grillbot.Models
{
    public class GroupCommandMatch
    {
        public int? MethodID { get; set; }
        public string Group { get; set; }
        public string Command { get; set; }

        public override string ToString()
        {
            return $"{Group}/{Command}";
        }
    }
}
