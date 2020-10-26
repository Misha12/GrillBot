namespace Grillbot.Models
{
    public class PaginationInfo
    {
        public int Page { get; set; } = 1;
        public bool CanPrev { get; set; }
        public bool CanNext { get; set; }
    }
}
