using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Slicey.Net.StateStore;

namespace Slicey.Net
{
    public static class ServiceRegistration
    {
        public static void RegisterRootStore<TStore, TStoreType>(this IHostBuilder builder, TStore store) where TStore : RootStateStore<TStoreType> 
        {
            builder.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<RootStateStore<TStoreType>>(store);
            });
        }

        public static void RegisterSliceStore<TStore, TRootType, TStoreType>(this IHostBuilder builder, TStore store) where TStore : SliceStateStore<TRootType, TStoreType>
        {
            builder.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<SliceStateStore<TRootType, TStoreType>>(store);
            });
        }
    }
}
