using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace EFCore.Extension.Context
{
    internal interface IUnitOfWorK
    {
        DataContext Contexts(string key);

        DataContext Context { get; set; }
    }


    internal class UnitOfWorK : IUnitOfWorK, IDisposable
    {
        private DataContext _Context;

        public DataContext Context
        {
            set { _Context = value; }
            get
            {
                if (_Context != null && !_Context.isDispose)
                    return _Context;
                else
                {
                    _Context = new DataContext(EFCoreCache.Config.FirstOrDefault().Value?.Key);
                    return _Context;
                }
            }
        }

        private readonly ConcurrentDictionary<string, DataContext> list = new ConcurrentDictionary<string, DataContext>();

        public void Dispose()
        {
            foreach (var item in list)
            {
                if (!item.Value.isDispose)
                    _Context?.Dispose();
                item.Value?.Dispose();
            }
            list.Clear();
        }

        public DataContext Contexts(string key)
        {
            if (!EFCoreCache.Config.Keys.ToList().Exists(a => a == key) && EFCoreCache.Config.Count > 1)
                throw new Exception($"不存在数据库Key:{key}");

            if (string.IsNullOrEmpty(key))
                key = EFCoreCache.Config.FirstOrDefault().Value?.Key;

            DataContext data;
            list.TryGetValue(key, out data);
            if (data == null || data?.isDispose == true)
            {
                list.TryRemove(key, out data);
                data = new DataContext(key);
                list.TryAdd(key, data);
            }
            return data;
        }
    }
}
