using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EFCore.Extension.Base
{
    internal static class BaseJson
    {
        internal static List<Dictionary<string, object>> DataReaderToDic(DbDataReader reader, bool isOracle = false)
        {
            var result = new List<Dictionary<string, object>>();
            var cols = GetCol(reader);

            while (reader.Read())
            {
                var dic = new Dictionary<string, object>();
                cols.ForEach(a =>
                {
                    if (reader[a] is DBNull)
                        dic.Add(a.ToLower(), "");
                    else if (isOracle)
                        ReadOracle(reader, a, dic);
                    else
                        dic.Add(a.ToLower(), reader[a]);
                });

                result.Add(dic);
            }

            return result;
        }

        private static void ReadOracle(DbDataReader reader, string a, Dictionary<string, object> dic)
        {
            var id = reader.GetOrdinal(a.ToUpper());
            var typeName = reader.GetDataTypeName(id).ToLower();
            if (typeName == "clob" || typeName == "nclob")
            {
                var temp = BaseEmit.Invoke(reader, reader.GetType().GetMethod("GetOracleClob"), new object[] { id });
                dic.Add(a.ToLower(), BaseEmit.Get(temp, "Value"));
                BaseEmit.Invoke(temp, temp.GetType().GetMethod("Close"), null);
                BaseEmit.Invoke(temp, temp.GetType().GetMethod("Dispose"), null);
            }
            else if (typeName == "blob")
            {
                var temp = BaseEmit.Invoke(reader, reader.GetType().GetMethod("GetOracleBlob"), new object[] { id });
                dic.Add(a.ToLower(), BaseEmit.Get(temp, "Value"));
                BaseEmit.Invoke(temp, temp.GetType().GetMethod("Close"), null);
                BaseEmit.Invoke(temp, temp.GetType().GetMethod("Dispose"), null);
            }
            else
                dic.Add(a.ToLower(), reader[a]);
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
    }
}
