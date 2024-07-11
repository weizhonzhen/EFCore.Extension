using EFCore.Extension.Aop;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using EFCore.Extension;

namespace EFCore.Extension.Model
{
    public class ConfigData
    {
        public string Key { get; set; }

        public string ProviderName { get; set; }

        public string FactoryClient { get; set; }

        public string ConnStr { get; set; }

        public IEFCoreAop Aop { get; set; }

        public DbContext Context { get; set; }

        public DbTypeEnum DbType { get; set; }
    }
}
