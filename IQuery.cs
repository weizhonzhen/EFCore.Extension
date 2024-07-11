using FastUntility.Core.Page;
using System.Collections.Generic;

namespace EFCore.Extension
{
    public abstract class IQuery
    {
        public abstract T ToItem<T>() where T : class, new();

        public abstract List<T> ToList<T>() where T : class, new();

        public abstract Dictionary<string, object> ToDic();

        public abstract List<Dictionary<string, object>> ToDics();

        public abstract dynamic ToDyn();

        public abstract List<dynamic> ToDyns();
        public abstract PageResult ToPage(PageModel pModel);

        public abstract PageResult<T> ToPage<T>(PageModel pModel) where T : class, new();

        public abstract PageResultDyn ToPageDyn(PageModel pModel);

        public abstract int ToCount();

        public abstract IQuery ToTake(int count);
    }
}
