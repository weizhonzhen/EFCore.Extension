using EFCore.Extension.Base;
using EFCore.Extension.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Microsoft.EntityFrameworkCore
{
    public static class EFContext
    {
        public static EFCore.Extension.Query QuerySql(this DbContext context, string sql, List<DbParameter> param, string key = null)
        {
            var query = new EFCore.Extension.Query();
            query.Data.sql = sql;
            query.Data.param = param;
            query.Data.key = key;

            return query;
        }
    }

    internal class DataContext : IDisposable
    {
        private DbConnection conn;
        internal DbCommand cmd;
        internal ConfigData config;

        public DataContext(string key)
        {
            try
            {
                DbProviderFactory dbProviderFactory;

                if (key == null)
                {
                    dbProviderFactory = EFCoreCache.DbFactory.First().Value;
                    config = EFCoreCache.Config.First().Value;
                }
                else
                {
                    EFCoreCache.DbFactory.TryGetValue(key, out dbProviderFactory);
                    EFCoreCache.Config.TryGetValue(key, out config);
                }
                conn = dbProviderFactory.CreateConnection();
                conn.ConnectionString = config.ConnStr;
                conn.Open();
                cmd = conn.CreateCommand();
            }
            catch (Exception ex)
            {
                BaseAop.AopException(key, string.Empty, null, ex);
                throw new Exception(ex.Message);
            }
        }

        public void Dispose()
        {
            conn.Close();
            cmd.Dispose();
            conn.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}