using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EFCore.Extension.Base
{
    internal class PropertyCache
    {
        internal static readonly ConcurrentDictionary<string, List<PropertyModel>> PropertyInfo = new ConcurrentDictionary<string, List<PropertyModel>>();

        internal static List<PropertyModel> GetPropertyInfo<T>()
        {
            var list = new List<PropertyModel>();
            var key = string.Format("{0}.{1}", typeof(T).Namespace, typeof(T).Name);

            if (PropertyInfo.ContainsKey(key))
                PropertyInfo.TryGetValue(key, out list);
            else
            {
                typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList().ForEach(a =>
                {
                    if (!a.GetMethod.IsVirtual)
                    {
                        var temp = new PropertyModel();
                        temp.Name = a.Name;
                        temp.PropertyType = a.PropertyType;
                        list.Add(temp);
                    }
                });
                PropertyInfo.TryAdd(key, list);
            }

            return list;
        }
    }

    internal class PropertyModel
    {
        public string Name { get; set; }

        public System.Type PropertyType { get; set; }
    }
}
