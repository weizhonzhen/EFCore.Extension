namespace EFCore.Extension.Page
{
    public class PageModel
    {
        public int StarId { get; set; }

        public int EndId { get; set; }

        public int TotalRecord { get; set; }

        public int TotalPage { get; set; }

        public int PageId { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
