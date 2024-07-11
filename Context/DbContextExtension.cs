using FastUntility.Core.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace EFCore.Extension.Context
{
    internal class DbContextCache
    {
        internal static readonly ConcurrentDictionary<string, List<Type>> regType = new ConcurrentDictionary<string, List<Type>>();
        internal static readonly ConcurrentDictionary<string, List<PropertyInfo>> keyList = new ConcurrentDictionary<string, List<PropertyInfo>>();
        internal static readonly ConcurrentDictionary<string, MethodInfo> modelMethod = new ConcurrentDictionary<string, MethodInfo>();
        internal static readonly ConcurrentDictionary<string, MethodInfo> keyMethod = new ConcurrentDictionary<string, MethodInfo>();
    }

    public class DbContextExtension : DbContext
    {
        public DbContextExtension(DbContextOptions<DbContextExtension> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            try
            {
                List<Type> regType;
                DbContextCache.regType.TryGetValue("regType", out regType);

                if (regType == null)
                {
                    regType = Assembly.GetExecutingAssembly().GetTypes().Where(p => !string.IsNullOrEmpty(p.Namespace)).Where(p => !string.IsNullOrEmpty(p.GetCustomAttribute<TableAttribute>()?.Name)).ToList();
                    DbContextCache.regType.TryAdd("regType", regType);
                }

                regType.ForEach(type =>
                {
                    dynamic configurationInstance = Activator.CreateInstance(type);
                    modelBuilder.Model.AddEntityType(type);
                });

                var entityMethod = typeof(ModelBuilder).GetMethod("Entity", new Type[] { });

                var typeList = regType.Where(o => o.GetCustomAttributes<TableAttribute>().Any())
                                  .Where(o => o.IsClass && !o.IsAbstract && !o.IsGenericType).ToList();

                typeList.ForEach(type =>
                {
                    List<PropertyInfo> key;
                    DbContextCache.keyList.TryGetValue(type.FullName, out key);
                    if (key == null)
                    {
                        key = type.GetProperties().Where(a => a.GetCustomAttributes<KeyAttribute>().Any()).ToList();
                        DbContextCache.keyList.TryAdd(type.FullName, key);
                    }

                    if (key != null && key.Count > 0)
                    {
                        object model = null;
                        MethodInfo modelMethod;
                        DbContextCache.modelMethod.TryGetValue(type.FullName, out modelMethod);
                        if (modelMethod == null)
                        {
                            modelMethod = entityMethod.MakeGenericMethod(type);
                            DbContextCache.modelMethod.TryAdd(type.FullName, modelMethod);
                        }

                        model = BaseEmit.Invoke(modelBuilder, modelMethod, new object[] { });

                        MethodInfo keyMethod;
                        DbContextCache.keyMethod.TryGetValue(type.FullName, out keyMethod);
                        if (keyMethod == null)
                        {
                            keyMethod = model.GetType().GetMethod("HasKey", new Type[] { typeof(string[]) });
                            DbContextCache.keyMethod.TryAdd(type.FullName, keyMethod);
                        }
                        BaseEmit.Invoke(model, keyMethod, new object[] { key.Select(a => a.Name).ToArray() });
                    }
                });

                base.OnModelCreating(modelBuilder);
            }
            catch { }
        }
    }
}
