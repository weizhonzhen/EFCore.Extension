using System.ComponentModel;

namespace EFCore.Extension
{

    public enum DbTypeEnum
    {
        [Description("default")]
        Default = 0,        

        [Description("MySql")]
        MySql = 1,

        [Description("SqlServer")]
        SqlServer = 2,

        [Description("DB2")]
        DB2 = 3,

        [Description("SQLite")]
        SQLite = 4,

        [Description("PostgreSql")]
        PostgreSql = 5,

        [Description("Oracle")]
        Oracle = 6,
    }
}
