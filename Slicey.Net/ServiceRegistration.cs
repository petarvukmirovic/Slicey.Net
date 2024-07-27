using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Slicey.Net.StateStore;
using System.Xml;

namespace Slicey.Net
{
    public static class ServiceRegistration
    {
        public static void RegisterRootStore<TStore, TStoreType>(this IHostBuilder builder, TStoreType initialData) where TStore : RootStateStore<TStoreType> =>
            builder.ConfigureServices((_, services) => services.RegisterRootStore<TStore, TStoreType>(initialData));

        public static void RegisterSliceStore<TStore, TRootType, TStoreType>(this IHostBuilder builder) where TStore : SliceStateStore<TRootType, TStoreType> => 
            builder.ConfigureServices((ctx, services) => services.RegisterSliceStore<TStore, TRootType, TStoreType>());

        public static void RegisterRootStore<TStore, TStoreType>(this IHostApplicationBuilder builder, TStoreType initialData) where TStore : RootStateStore<TStoreType> =>
            builder.Services.RegisterRootStore<TStore, TStoreType>(initialData);

        public static void RegisterSliceStore<TStore, TRootType, TStoreType>(this IHostApplicationBuilder builder) where TStore : SliceStateStore<TRootType, TStoreType> =>
            builder.Services.RegisterSliceStore<TStore, TRootType, TStoreType>();

        public static void RegisterRootStore<TStore, TStoreType>(this IServiceCollection services, TStoreType initialData) where TStore : RootStateStore<TStoreType>
        {
            services.AddSingleton<TStore>(x =>
                    (TStore?)Activator.CreateInstance(typeof(TStore), initialData)
                    ?? throw new InvalidOperationException($"type {typeof(TStore)} must have a public constructor that accepts initial data of type {typeof(TStoreType)}"));
            services.AddSingleton<RootStateStore<TStoreType>>(x => x.GetRequiredService<TStore>());
        }

        public static void RegisterSliceStore<TStore, TRootType, TStoreType>(this IServiceCollection services) where TStore : SliceStateStore<TRootType, TStoreType>
        {
            services.AddSingleton(x =>
                    (TStore?)Activator.CreateInstance(typeof(TStore), x.GetRequiredService<RootStateStore<TRootType>>()) ??
                    throw new InvalidOperationException($"slice store {typeof(TStore)}  must have a public constructor that accepts root store of type {typeof(RootStateStore<TRootType>)}"));
        }

    }
}
