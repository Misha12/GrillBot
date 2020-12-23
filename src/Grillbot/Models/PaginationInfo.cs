namespace Grillbot.Models
{
    public class PaginationInfo
    {
        public const int DefaultPageSize = 25;

        public int Page { get; set; } = 1;
        public bool CanPrev { get; set; }
        public bool CanNext { get; set; }
        public int PagesCount { get; set; }
        public int TotalCount { get; set; }
        public string PaginationKey { get; set; }
        public bool WithCounts { get; set; }

        public PaginationInfo() { }

        public PaginationInfo(int skip, int page, int totalCount)
        {
            Page = page;
            CanNext = skip + DefaultPageSize < totalCount;
            CanPrev = skip != 0;
            PagesCount = System.Math.Max((int)System.Math.Ceiling(totalCount / (double)DefaultPageSize), 1);
            TotalCount = totalCount;
        }
    }
}
