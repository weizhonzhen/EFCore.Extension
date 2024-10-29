using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;


namespace EFCore.Extension.Base
{
    internal static class BaseConfig
    {
        internal static List<T> GetListValue<T>(string key, string fileName = "appsettings.json") where T : class, new()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile(fileName, optional: true, reloadOnChange: true);
            var config = build.Build();
            var list = new ServiceCollection().AddOptions().Configure<List<T>>(config.GetSection(key)).BuildServiceProvider().GetService<IOptions<List<T>>>().Value;

            return list;
        }
    }
}
