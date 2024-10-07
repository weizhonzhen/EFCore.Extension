using EFCore.Extension.Aop;
using EFCore.Extension.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using EFCore.Extension.Context;
using FastUntility.Core;
using FastUntility.Core.Base;
using NPOI.OpenXmlFormats.Dml;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class EFCoreCache
    {
        internal static readonly Dictionary<string, DbProviderFactory> DbFactory = new Dictionary<string, DbProviderFactory>();
        internal static readonly Dictionary<string, ConfigData> Config = new Dictionary<string, ConfigData>();
    }

    public static class EFCoreExtension
    {
        private static readonly string Config = "EFConfig";
        public static IServiceCollection AddEfCore(this IServiceCollection serviceCollection, Action<ConfigData> action)
        {
            var config = new ConfigData();
            action(config);

            if (string.IsNullOrEmpty(config.ProviderName))
                throw new System.Exception("services.AddEfCore ProviderName not null");

            if (string.IsNullOrEmpty(config.FactoryClient))
                throw new System.Exception("services.AddEfCore FactoryClient not null");

            if (string.IsNullOrEmpty(config.Key))
                throw new System.Exception("services.AddEfCore Key not null");

            if (config.Context == null)
                throw new System.Exception("services.AddEfCore Context not null");

            if ((int)config.DbType == 0)
                throw new System.Exception("services.AddEfCore DbType not null");

            if (string.IsNullOrEmpty(config.ConnStr))
                throw new System.Exception("services.AddEfCore ConnStr not null");

            var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == config.ProviderName);
            if (assembly == null)
                throw new System.Exception($"{config.ProviderName} not in Current Domain Assembly");

            var model = assembly.GetType(config.FactoryClient, false);

            if (model == null)
                throw new System.Exception($"{config.FactoryClient} not in Assembly {assembly.FullName}");

            var field = model.GetField("Instance");
            var instance = field.GetValue(model);

            DbConnection connection = null;
            try
            {
                connection = (instance as DbProviderFactory).CreateConnection();
            }
            catch (Exception ex)
            {
                throw new System.Exception($"{config.ConnStr} {ex.Message}");
            }
            finally
            {
                connection.Dispose();
            }

            if (config.Aop != null)
                serviceCollection.AddSingleton<IEFCoreAop>(config.Aop);

            EFCoreCache.DbFactory.Remove(config.Key);
            EFCoreCache.DbFactory.Add(config.Key, instance as DbProviderFactory);

            EFCoreCache.Config.Remove(config.Key);
            EFCoreCache.Config.Add(config.Key, config);

            serviceCollection.AddScoped<IUnitOfWorK, UnitOfWorK>();
            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
            return serviceCollection;
        }

        public static IServiceCollection AddEfCoreJosn(this IServiceCollection serviceCollection, Action<ConfigData> action)
        {
           var config = new ConfigData();
            action(config);

            var list = BaseConfig.GetListValue<ConfigData>(Config, config.DbFile);
            if (list.Count > 0)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                list.ForEach(a => {
                    a.Aop = config.Aop;
                    a.Context = config.Context;

                    if (string.IsNullOrEmpty(a.ProviderName))
                        throw new System.Exception("services.AddEfCore ProviderName not null");

                    if (string.IsNullOrEmpty(a.FactoryClient))
                        throw new System.Exception("services.AddEfCore FactoryClient not null");

                    if (string.IsNullOrEmpty(a.Key))
                        throw new System.Exception("services.AddEfCore Key not null");

                    if (a.Context == null)
                        throw new System.Exception("services.AddEfCore Context not null");

                    if ((int)a.DbType == 0)
                        throw new System.Exception("services.AddEfCore DbType not null");

                    if (string.IsNullOrEmpty(a.ConnStr))
                        throw new System.Exception("services.AddEfCore ConnStr not null");

                    var assembly = assemblies.Find(b => b.FullName.Split(',')[0] == a.ProviderName);
                    if (assembly == null)
                        throw new System.Exception($"{a.ProviderName} not in Current Domain Assembly");

                    var model = assembly.GetType(a.FactoryClient, false);

                    if (model == null)
                        throw new System.Exception($"{a.FactoryClient} not in Assembly {assembly.FullName}");

                    var field = model.GetField("Instance");
                    var instance = field.GetValue(model);

                    DbConnection connection = null;
                    try
                    {
                        connection = (instance as DbProviderFactory).CreateConnection();
                    }
                    catch (Exception ex)
                    {
                        throw new System.Exception($"{a.ConnStr} {ex.Message}");
                    }
                    finally
                    {
                        connection.Dispose();
                    }

                    if (a.Aop != null)
                        serviceCollection.AddSingleton<IEFCoreAop>(a.Aop);

                    EFCoreCache.DbFactory.Remove(a.Key);
                    EFCoreCache.DbFactory.Add(a.Key, instance as DbProviderFactory);

                    EFCoreCache.Config.Remove(a.Key);
                    EFCoreCache.Config.Add(a.Key, a);
                });

                serviceCollection.AddScoped<IUnitOfWorK, UnitOfWorK>();
            }
            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
            return serviceCollection;
        }
    }
}