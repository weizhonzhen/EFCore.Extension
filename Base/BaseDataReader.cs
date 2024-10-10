using FastUntility.Core.Base;
using FastUntility.Core.BuilderMethod;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;

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
                
                colList.ForEach(a =>
                {
                    dic[a] = dr[a];
                });

                list.Add(item);
            }

            return list;
        }

        private static List<string> GetCol(DbDataReader dr)
        {
            var list = new List<string>();
            for (var i = 0; i < dr.FieldCount; i++)
            {
                var colName = dr.GetName(i);
                if (!list.Exists(a => string.Compare(a, colName, true) == 0))
                    list.Add(colName);
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
                colList.ForEach(a =>
                {
                    if (dr[a] is DBNull)
                        return;
                    else
                    {
                        var info = propertyList.Find(b => string.Compare(b.Name, a, true) == 0);
                        if (info != null)
                            item = SetValue<T>(item, dr[a], info);
                    }
                });
                list.Add(item);
            }

            return list;
        }


        private static T SetValue<T>(T item, object value, PropertyModel info)
        {
            try
            {
                BaseEmit.Set(item, info.Name, value);
                return item;
            }
            catch { return item; }
        }
    }
}
