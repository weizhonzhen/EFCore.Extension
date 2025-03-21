using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EFCore.Extension.Aop
{
    public interface IEFCoreAop
    {
        void Exception(ExceptionContext context);

        void Before(BeforeContext context);

        void After(AfterContext context);
    }

    public class ExceptionContext
    {
        public string key { get; internal set; }

        public string sql { get; internal set; }

        public List<DbParameter> param {  get; internal set; }

        public Exception ex { get; internal set; }
    }

    public class BeforeContext
    {
        public DbTypeEnum dbType { get; internal set; }

        public string key { get; internal set; }

        public string sql { get; internal set; }

        public List<DbParameter> param { get; internal set; }
    }

    public class AfterContext
    {
        public DbTypeEnum dbType { get; internal set; }

        public string key { get; internal set; }

        public string sql { get; internal set; }

        public List<DbParameter> param { get; internal set; }

        public object Result { get; internal set; }
    }
}
