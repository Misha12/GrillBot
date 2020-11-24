namespace Grillbot.Models
{
    public class PaginationInfo
    {
        public const int DefaultPageSize = 25;

        public int Page { get; set; } = 1;
        public bool CanPrev { get; set; }
        public bool CanNext { get; set; }
        public int PagesCount { get; set; }
    }
}
