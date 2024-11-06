using EFCore.Extension.Aop;
using EFCore.Extension.Base;
using EFCore.Extension.Context;
using EFCore.Extension.Model;
using EFCore.Extension.Page;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace EFCore.Extension
{
    public sealed class Query : IQuery
    {
        internal DataQuery Data { get; set; } = new DataQuery();

        public override int ToCount(DataContext db = null)
        {
            try
            {
                db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(Data.key) : db;
                db.cmd.CommandText = $"select count(0) from ({Data.sql}) a";
                db.Dispose(db.cmd);
                db.cmd.Parameters.AddRange(Data.param.ToArray());
                BaseAop.AopBefore(db, db.cmd.CommandText, Data.param);
                var dt = BaseExecute.ToDataTable(db.cmd, db.cmd.CommandText);
                if (dt.Rows.Count > 0)
                {
                    BaseAop.AopAfter(db, db.cmd.CommandText, Data.param, dt.Rows[0][0].ToString().ToInt(0));
                    return dt.Rows[0][0].ToString().ToInt(0);
                }
                else
                {
                    BaseAop.AopAfter(db, db.cmd.CommandText, Data.param, 0);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                BaseAop.AopException(Data.key, Data.sql, Data.param, ex);
                LogEx("ToCount", ex);
                return 0;
            }
        }

        public override Dictionary<string, object> ToDic(DataContext db = null)
        {
            try
            {
                db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(Data.key) : db;
                BaseExecute.SetCommandText(1, db, Data);
                db.Dispose(db.cmd);
                db.cmd.Parameters.AddRange(Data.param.ToArray());
                BaseAop.AopBefore(db, db.cmd.CommandText, Data.param);
                using (var reader = db.cmd.ExecuteReader())
                {
                    var result = BaseJson.DataReaderToDic(reader, db.config.DbType == DbTypeEnum.Oracle).FirstOrDefault() ?? new Dictionary<string, object>();
                    BaseAop.AopAfter(db, db.cmd.CommandText, Data.param, result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                BaseAop.AopException(Data.key, Data.sql, Data.param, ex);
                LogEx("ToDic", ex);
                return new Dictionary<string, object>();
            }
        }

        public override List<Dictionary<string, object>> ToDics(DataContext db = null)
        {
            try
            {
                db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(Data.key) : db;
                if (Data.Take == 0)
                    db.cmd.CommandText = Data.sql;
                else
                    BaseExecute.SetCommandText(Data.Take, db, Data);
                db.Dispose(db.cmd);
                db.cmd.Parameters.AddRange(Data.param.ToArray());
                BaseAop.AopBefore(db, db.cmd.CommandText, Data.param);
                using (var reader = db.cmd.ExecuteReader())
                {
                    var result = BaseJson.DataReaderToDic(reader, db.config.DbType == DbTypeEnum.Oracle);
                    BaseAop.AopAfter(db, db.cmd.CommandText, Data.param, result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                BaseAop.AopException(Data.key, Data.sql, Data.param, ex);
                LogEx("ToDics", ex);
                return new List<Dictionary<string, object>>();
            }
        }

        public override dynamic ToDyn(DataContext db = null)
        {
            try
            {
                db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(Data.key) : db;
                BaseExecute.SetCommandText(1, db, Data);
                db.Dispose(db.cmd);
                db.cmd.Parameters.AddRange(Data.param.ToArray());
                BaseAop.AopBefore(db, db.cmd.CommandText, Data.param);
                using (var reader = db.cmd.ExecuteReader())
                {
                    var result = BaseDataReader.ToDyns(reader).FirstOrDefault() ?? new ExpandoObject();
                    BaseAop.AopAfter(db, db.cmd.CommandText, Data.param, result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                BaseAop.AopException(Data.key, Data.sql, Data.param, ex);
                LogEx("ToDyn", ex);
                return new ExpandoObject();
            }
        }

        public override List<dynamic> ToDyns(DataContext db = null)
        {
            try
            {
                db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(Data.key) : db;
                if (Data.Take == 0)
                    db.cmd.CommandText = Data.sql;
                else
                    BaseExecute.SetCommandText(Data.Take, db, Data);
                db.Dispose(db.cmd);
                db.cmd.Parameters.AddRange(Data.param.ToArray());
                BaseAop.AopBefore(db, db.cmd.CommandText, Data.param);
                using (var reader = db.cmd.ExecuteReader())
                {
                    var result = BaseDataReader.ToDyns(reader) ?? new List<dynamic>();
                    BaseAop.AopAfter(db, db.cmd.CommandText, Data.param, result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                BaseAop.AopException(Data.key, Data.sql, Data.param, ex);
                LogEx("ToDyns", ex);
                return new List<dynamic>();
            }
        }

        public override T ToItem<T>(DataContext db = null)
        {
            try
            {
                db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(Data.key) : db;
                BaseExecute.SetCommandText(1, db, Data);
                db.Dispose(db.cmd);
                db.cmd.Parameters.AddRange(Data.param.ToArray());
                BaseAop.AopBefore(db, db.cmd.CommandText, Data.param);
                using (var dr = BaseExecute.ToDataReader(db.cmd, db.cmd.CommandText))
                {
                    var result = BaseDataReader.ToItem<T>(dr) ?? new T();
                    BaseAop.AopAfter(db, db.cmd.CommandText, Data.param, result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                BaseAop.AopException(Data.key, Data.sql, Data.param, ex);
                LogEx("ToItem", ex);
                return new T();
            }
        }

        public override List<T> ToList<T>(DataContext db = null)
        {
            try
            {
                db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(Data.key) : db;
                if (Data.Take == 0)
                    db.cmd.CommandText = Data.sql;
                else
                    BaseExecute.SetCommandText(Data.Take, db, Data);
                db.Dispose(db.cmd);
                db.cmd.Parameters.AddRange(Data.param.ToArray());
                BaseAop.AopBefore(db, db.cmd.CommandText, Data.param);
                using (var dr = BaseExecute.ToDataReader(db.cmd, db.cmd.CommandText))
                {
                    var result = BaseDataReader.ToList<T>(dr) ?? new List<T>();
                    BaseAop.AopAfter(db, db.cmd.CommandText, Data.param, result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                BaseAop.AopException(Data.key, Data.sql, Data.param, ex);
                LogEx("ToList", ex);
                return new List<T>();
            }
        }

        public override PageResult ToPage(PageModel pModel, DataContext db = null)
        {
            var result = new PageResult();
            try
            {
                db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(Data.key) : db;
                db.Dispose(db.cmd);
                db.cmd.Parameters.AddRange(Data.param.ToArray());
                pModel.StarId = (pModel.PageId - 1) * pModel.PageSize + 1;
                pModel.EndId = pModel.PageId * pModel.PageSize;
                pModel.TotalRecord = ToCount(db);

                if (pModel.TotalRecord > 0)
                {
                    if ((pModel.TotalRecord % pModel.PageSize) == 0)
                        pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                    else
                        pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;

                    if (pModel.PageId > pModel.TotalPage)
                        pModel.PageId = pModel.TotalPage;

                    BaseExecute.SetCommandText(db, pModel, Data.sql);
                    BaseAop.AopBefore(db, db.cmd.CommandText, Data.param);

                    using (var dr = BaseExecute.ToDataReader(db.cmd, db.cmd.CommandText))
                    {
                        result.list = BaseJson.DataReaderToDic(dr, db.config.DbType == DbTypeEnum.Oracle);
                        BaseAop.AopAfter(db, db.cmd.CommandText, Data.param, result.list);
                    }
                }
                else
                    result.list = new List<Dictionary<string, object>>();

                result.pModel = pModel;

                return result;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(Data.key, Data.sql, Data.param, ex);
                LogEx("ToPage", ex);
                return new PageResult();
            }
        }

        public override PageResult<T> ToPage<T>(PageModel pModel, DataContext db = null)
        {
            var result = new PageResult<T>();
            try
            {
                db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(Data.key) : db;
                db.Dispose(db.cmd);
                db.cmd.Parameters.AddRange(Data.param.ToArray());
                pModel.StarId = (pModel.PageId - 1) * pModel.PageSize + 1;
                pModel.EndId = pModel.PageId * pModel.PageSize;
                pModel.TotalRecord = ToCount(db);

                if (pModel.TotalRecord > 0)
                {
                    if ((pModel.TotalRecord % pModel.PageSize) == 0)
                        pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                    else
                        pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;

                    if (pModel.PageId > pModel.TotalPage)
                        pModel.PageId = pModel.TotalPage;

                    BaseExecute.SetCommandText(db, pModel, Data.sql);
                    BaseAop.AopBefore(db, db.cmd.CommandText, Data.param);

                    using (var dr = BaseExecute.ToDataReader(db.cmd, db.cmd.CommandText))
                    {
                        result.list = BaseDataReader.ToList<T>(dr);
                        BaseAop.AopAfter(db, db.cmd.CommandText, Data.param, result.list);
                    }
                }
                else
                    result.list = new List<T>();

                result.pModel = pModel;

                return result;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(Data.key, Data.sql, Data.param, ex);
                LogEx("ToPage", ex);
                return new PageResult<T>();
            }
        }

        public override PageResultDyn ToPageDyn(PageModel pModel, DataContext db = null)
        {
            var result = new PageResultDyn();
            try
            {
                db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(Data.key) : db;
                db.Dispose(db.cmd);
                db.cmd.Parameters.AddRange(Data.param.ToArray());
                pModel.StarId = (pModel.PageId - 1) * pModel.PageSize + 1;
                pModel.EndId = pModel.PageId * pModel.PageSize;
                pModel.TotalRecord = ToCount(db);

                if (pModel.TotalRecord > 0)
                {
                    if ((pModel.TotalRecord % pModel.PageSize) == 0)
                        pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                    else
                        pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;

                    if (pModel.PageId > pModel.TotalPage)
                        pModel.PageId = pModel.TotalPage;

                    BaseExecute.SetCommandText(db, pModel, Data.sql);
                    BaseAop.AopBefore(db, db.cmd.CommandText, Data.param);

                    using (var dr = BaseExecute.ToDataReader(db.cmd, db.cmd.CommandText))
                    {
                        result.list = BaseDataReader.ToDyns(dr);
                        BaseAop.AopAfter(db, db.cmd.CommandText, Data.param, result.list);
                    }
                }
                else
                    result.list = new List<dynamic>();

                result.pModel = pModel;

                return result;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(Data.key, Data.sql, Data.param, ex);
                LogEx("ToPageDyn", ex);
                return new PageResultDyn();
            }
        }

        public override IQuery ToTake(int count)
        {
            Data.Take = count;
            return this;
        }

        private void LogEx(string method, Exception ex)
        {
            var aop = ServiceContext.Engine.Resolve<IEFCoreAop>();
            if (aop != null)
            {
                var context = new ExceptionContext();
                context.key = Data.key;
                context.sql = Data.sql;
                context.ex = ex;
                aop.Exception(context);
            }
        }
    }
}