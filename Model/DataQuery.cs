using System.Collections.Generic;
using System.Data.Common;

namespace EFCore.Extension.Model
{
    internal class DataQuery
    {
        internal string sql {  get; set; }

        internal List<DbParameter> param { get; set; }

        internal string key { get; set; }  

        internal int Take { get; set; }
    }
}
