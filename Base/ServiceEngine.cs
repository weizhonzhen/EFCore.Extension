using System;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Extension.Base
{
    internal class ServiceEngine : IServiceEngine
    {
        private IServiceProvider serviceProvider;
        public ServiceEngine(IServiceProvider _serviceProvider)
        {
            this.serviceProvider = _serviceProvider;
        }

        public T Resolve<T>()
        {
            if (serviceProvider == null)
                return default(T);
            else
                return serviceProvider.GetService<T>();
        }
    }

    internal interface IServiceEngine
    {
        T Resolve<T>();
    }

    internal class ServiceContext
    {
        private static IServiceEngine engine = new ServiceEngine(null);
        internal static IServiceEngine Init(IServiceEngine _engine)
        {
            engine = _engine;
            return engine;
        }

        public static IServiceEngine Engine
        {
            get
            {
                return engine;
            }
        }
    }
}