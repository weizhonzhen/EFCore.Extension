using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System;
using FastUntility.Core.Base;

namespace EFCore.Extension.Base
{
    internal class BaseDataReader
    {
        public static List<dynamic> ToDyns(DbDataReader dr)
        {
            var list = new List<dynamic>();
            var colList = new List<string>();

            if (dr == null)
                return list;

            if (dr.HasRows)
                colList = GetCol(dr);

            while (dr.Read())
            {
                dynamic item = new ExpandoObject();
                var dic = (IDictionary<string, object>)item;

                foreach (var key in colList)
                {
                    dic[key] = dr[key];
                }

                list.Add(item);
            }

            return list;
        }

        private static List<string> GetCol(DbDataReader dr)
        {
            var list = new List<string>();
            for (var i = 0; i < dr.FieldCount; i++)
            {
                list.Add(dr.GetName(i));
            }
            return list;
        }

        public static List<T> ToList<T>(DbDataReader dr) where T : class, new()
        {
            var list = new List<T>();
            var colList = new List<string>();

            if (dr == null)
                return list;

            if (dr.HasRows)
                colList = GetCol(dr);

            var propertyList = PropertyCache.GetPropertyInfo<T>();

            while (dr.Read())
            {
                var item = new T();
                propertyList.ForEach(info =>
                {
                    if (!colList.Exists(a => string.Compare(a, info.Name, true) == 0))
                        return;

                    if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                        return;

                    item = SetValue<T>(item, dr, info);
                });
                list.Add(item);
            }

            return list;
        }


        private static T SetValue<T>(T item, DbDataReader dr, PropertyModel info)
        {
            try
            {
                var id = dr.GetOrdinal(info.Name);
                if (!dr.IsDBNull(id))
                    BaseEmit.Set(item, info.Name, dr.GetValue(id));

                return item;
            }
            catch { return item; }
        }
    }
}
