using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace OrdersDemo.Features.Products;

public static class ProductsServiceCollectionExtensions
{
    /// <summary>Meter published by <see cref="MemoryCache"/> when statistics tracking is enabled.</summary>
    public const string CacheMeterName = "Microsoft.Extensions.Caching.Memory.MemoryCache";

    public const string CacheName = "products";

    public static IServiceCollection AddProducts(this IServiceCollection services)
    {
        services.AddOptions<ProductCacheOptions>().BindConfiguration(ProductCacheOptions.SectionName);

        services.AddMemoryCache();
        services.AddSingleton<IConfigureOptions<MemoryCacheOptions>, ConfigureProductMemoryCache>();

        services.AddSingleton<IProductStore, InMemoryProductStore>();
        services.AddSingleton<IProductCatalog, CachedProductCatalog>();
        return services;
    }

    private sealed class ConfigureProductMemoryCache(IOptions<ProductCacheOptions> productCache)
        : IConfigureOptions<MemoryCacheOptions>
    {
        public void Configure(MemoryCacheOptions options)
        {
            options.TrackStatistics = true;        // opt-in: without this no instruments are published
            options.Name = CacheName;              // becomes the dotnet.cache.name tag
            options.SizeLimit = productCache.Value.SizeLimit;
            options.CompactionPercentage = productCache.Value.CompactionPercentage;
        }
    }
}
