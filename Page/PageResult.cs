using System.Collections.Generic;

namespace EFCore.Extension.Page
{
    public sealed class PageResult<T> where T : class, new()
    {
        public PageModel pModel = new PageModel();

        public List<T> list = new List<T>();
    }

    public class PageResult
    {
        public PageModel pModel = new PageModel();

        public List<Dictionary<string, object>> list { get; set; }
    }

    public class PageResultDyn
    {
        public PageModel pModel = new PageModel();

        public List<dynamic> list { get; set; }
    }
}
