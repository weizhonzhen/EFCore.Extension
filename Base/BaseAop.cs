using EFCore.Extension.Aop;
using EFCore.Extension.Context;
using FastUntility.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EFCore.Extension.Base
{
    internal static class BaseAop
    {
        public static void AopBefore(DataContext db, string sql, List<DbParameter> param)
        {
            var aop = ServiceContext.Engine.Resolve<IEFCoreAop>();
            if (aop != null)
            {
                var context = new BeforeContext();

                context.sql = sql;
                context.key = db.config.Key;
                context.dbType = db.config.DbType;

                if (param != null)
                    context.param = param;

                aop.Before(context);
            }
        }

        public static void AopAfter(DataContext db, string sql, List<DbParameter> param,object result)
        {
            var aop = ServiceContext.Engine.Resolve<IEFCoreAop>();
            if (aop != null)
            {
                var context = new AfterContext();

                context.sql = sql;
                context.key = db.config.Key;
                context.dbType = db.config.DbType;
                context.Result = result;

                if (param != null)
                    context.param = param;

                aop.After(context);

            }
        }

        public static void AopException(string key, string sql, List<DbParameter> param, Exception ex)
        {
            var aop = ServiceContext.Engine.Resolve<IEFCoreAop>();
            if (aop != null)
            {
                var context = new ExceptionContext();

                context.sql = sql;
                context.key = key;
                context.ex = ex;

                if (param != null)
                    context.param = param;

                aop.Exception(context);

            }
        }        
    }
}
