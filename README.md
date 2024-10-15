```
builder.Services.AddEfCoreJosn(a => {
    a.Key = "api";
    a.FactoryClient = "MySql.Data.MySqlClient.MySqlClientFactory";
    a.ConnStr = "Server=127.0.0.1;Port=8080;User Id=test;Password=test;Database=test;Character Set=utf8mb4;";
    a.ProviderName = "MySql.Data";
    a.Context = builder.Services.BuildServiceProvider().GetService<TestDbContext>();
    a.DbType = EFCore.Extension.DbTypeEnum.MySql;
    a.Aop = new SqlAop();
});
```


or 

```
db.json
{
  "EFConfig": [
    {
      "ProviderName": "MySql.Data",
      "DbType": "MySql",
      "ConnStr": "Server=127.0.0.1;Port=8080;User Id=test;Password=test;Database=test;Character Set=utf8mb4;",
      "FactoryClient": "MySql.Data.MySqlClient.MySqlClientFactory",
      "Key": "api"
    }
  ]
}

builder.Services.AddEfCoreJosn(a => {
    a.Context = builder.Services.BuildServiceProvider().GetService<TestDbContext>();;
});
```


```
var list = dbContext.QuerySql(sql, param, "api").ToList<Model>();
var dics = dbContext.QuerySql(sql, param, "api").ToDics();
var pageModel = new PageModel();
pageModel.PageId = 3;
var pageTList = dbContext.QuerySql(sql, param, "api").ToPage<JbBusiApplySalaryDetail>(pageModel);
```

```
aop
public class SqlAop : IEFCoreAop
{
    public void After(AfterContext context)
    {
        //throw new NotImplementedException();
    }

    public void Before(BeforeContext context)
    {
        //throw new NotImplementedException();
    }

    public void Exception(ExceptionContext context)
    {
        //throw new NotImplementedException();
    }
}
```
