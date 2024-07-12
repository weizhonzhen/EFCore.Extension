using FastUntility.Core.Page;
using System.Collections.Generic;
using EFCore.Extension.Context;

namespace EFCore.Extension
{
    public abstract class IQuery
    {
        public abstract T ToItem<T>(DataContext db = null) where T : class, new();

        public abstract List<T> ToList<T>(DataContext db = null) where T : class, new();

        public abstract Dictionary<string, object> ToDic(DataContext db = null);

        public abstract List<Dictionary<string, object>> ToDics(DataContext db = null);

        public abstract dynamic ToDyn(DataContext db = null);

        public abstract List<dynamic> ToDyns(DataContext db = null);
        public abstract PageResult ToPage(PageModel pModel, DataContext db = null);

        public abstract PageResult<T> ToPage<T>(PageModel pModel, DataContext db = null) where T : class, new();

        public abstract PageResultDyn ToPageDyn(PageModel pModel, DataContext db = null);

        public abstract int ToCount(DataContext db = null);

        public abstract IQuery ToTake(int count);
    }
}
