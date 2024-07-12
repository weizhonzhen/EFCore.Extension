using EFCore.Extension.Base;
using EFCore.Extension.Model;
using FastUntility.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace EFCore.Extension.Context
{
    public static class EFContext
    {
        public static Query QuerySql(this DbContext context, string sql, List<DbParameter> param, string key = null)
        {
            var query = new Query();
            query.Data.sql = sql;
            query.Data.param = param;
            query.Data.key = key;

            return query;
        }
    }

    public class DataContext : IDisposable
    {
        private DbConnection conn;
        internal DbCommand cmd;
        internal ConfigData config;
        internal bool isDispose;

        public DataContext(string key)
        {
            try
            {
                DbProviderFactory dbProviderFactory;

                if (key == null)
                {
                    dbProviderFactory = EFCoreCache.DbFactory.FirstOrDefault().Value;
                    config = EFCoreCache.Config.FirstOrDefault().Value;
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
            Dispose(cmd);
            conn.Close();
            cmd.Dispose();
            conn.Dispose();
            isDispose = true;
            GC.SuppressFinalize(this);
        }

        internal void Dispose(DbCommand cmd)
        {
            if (cmd == null) return;
            if (cmd.Parameters != null && config.DbType == DbTypeEnum.Oracle)
            {
                foreach (var param in cmd.Parameters)
                {
                    param.GetType().GetMethods().ToList().ForEach(m =>
                    {
                        if (m.Name == "Dispose")
                            BaseEmit.Invoke(cmd, m, null);
                    });
                }
            }
            cmd.Parameters.Clear();
        }
    }
}