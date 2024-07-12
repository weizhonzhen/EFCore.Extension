using EFCore.Extension.Context;
using EFCore.Extension.Model;
using FastUntility.Core.Page;
using System.Data;
using System.Data.Common;

namespace EFCore.Extension.Base
{
    internal class BaseExecute
    {
        public static DataTable ToDataTable(DbCommand cmd, string sql, bool IsProcedure = false)
        {
            var dt = new DataTable();
            using (var dr = ToDataReader(cmd, sql, IsProcedure))
            {
                dt.Load(dr);
                dr.Close();
                dr.Dispose();
                return dt;
            }
        }

        public static DbDataReader ToDataReader(DbCommand cmd, string sql, bool IsProcedure = false)
        {
            if (IsProcedure)
                cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = sql;
            return cmd.ExecuteReader();
        }

        public static void SetCommandText(int count, DataContext db,DataQuery data)
        {
            if (db.config.DbType == DbTypeEnum.Oracle)
                db.cmd.CommandText = $"select * from ({data.sql})a where rownum <={count}";

            if (db.config.DbType == DbTypeEnum.DB2)
                db.cmd.CommandText = $"select * from ({data.sql})a fetch first {count} rows only";

            if (db.config.DbType == DbTypeEnum.MySql)
                db.cmd.CommandText = $"select * from ({data.sql})a limit {count}";

            if (db.config.DbType == DbTypeEnum.PostgreSql)
                db.cmd.CommandText = $"select * from ({data.sql})a limit {count}";

            if (db.config.DbType == DbTypeEnum.SQLite)
                db.cmd.CommandText = $"select * from ({data.sql})a limit 0 offset {count}";
        }

        public static void SetCommandText(DataContext db, PageModel pModel, string sql)
        {
            if (db.config.DbType == DbTypeEnum.SqlServer)
                db.cmd.CommandText = $"select * from ({sql})a offset({pModel.StarId - 1} * {pModel.PageSize}) rows fetch next {pModel.PageSize} rows only";
            else if (db.config.DbType == DbTypeEnum.Oracle)
                db.cmd.CommandText = $"select * from(select field.*,ROWNUM RN from({sql}) field where rownum<={pModel.EndId}) where rn>={pModel.StarId}";
            else if (db.config.DbType == DbTypeEnum.MySql)
                db.cmd.CommandText = $"select * from ({sql}) field limit {pModel.StarId}, {pModel.PageSize}";
            else if (db.config.DbType == DbTypeEnum.DB2)
                db.cmd.CommandText = $"select * from ({sql})a fetch first {pModel.StarId - 1} * {pModel.PageSize} rows only offset {pModel.PageSize} rows";
            else if (db.config.DbType == DbTypeEnum.SQLite)
                db.cmd.CommandText = $"select * from ({sql})a limit {pModel.StarId} offset {pModel.PageSize}";
            else if (db.config.DbType == DbTypeEnum.PostgreSql)
                db.cmd.CommandText = $"select * from ({sql})a limit {pModel.StarId} offset {pModel.PageSize}";
        }
    }
}
