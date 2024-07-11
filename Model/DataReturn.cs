using FastUntility.Core.Page;
using System.Collections.Generic;

namespace EFCore.Extension.Model
{
    public class DataReturn<T> where T : class, new()
    {
        public int Count { get; set; }

        public T Item { set; get; } = new T();

        public List<T> List { set; get; } = new List<T>();

        public PageResult<T> PageResult { set; get; } = new PageResult<T>();
    }

    public class DataReturnDyn
    {
        public int Count { get; set; }

        public dynamic Item { set; get; }

        public List<dynamic> List { set; get; } = new List<dynamic>();

        public PageResultDyn PageResult { set; get; } = new PageResultDyn();

        public dynamic WriteReturn { set; get; }
    }

    public class DataReturn
    {
        public int Count { get; set; }

        public List<Dictionary<string, object>> DicList { get; set; } = new List<Dictionary<string, object>>();

        public Dictionary<string, object> Dic { get; set; } = new Dictionary<string, object>();

        public PageResult PageResult = new PageResult();
    }
}
