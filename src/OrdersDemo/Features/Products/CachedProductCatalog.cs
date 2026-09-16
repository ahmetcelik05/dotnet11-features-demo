using Microsoft.Extensions.Caching.Memory;

namespace OrdersDemo.Features.Products;

/// <summary>
/// Read-through cache over <see cref="IProductStore"/>. With <see cref="MemoryCacheOptions.TrackStatistics"/> enabled,
/// the <c>Microsoft.Extensions.Caching.Memory.MemoryCache</c> meter reports hits, misses, evictions, entries and size.
/// </summary>
internal sealed class CachedProductCatalog(IMemoryCache cache, IProductStore store) : IProductCatalog
{
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(5);

    public Task<Product?> GetAsync(string sku, CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync(sku, entry =>
        {
            entry.SetSize(1); // required when MemoryCacheOptions.SizeLimit is set
            entry.SetSlidingExpiration(SlidingExpiration);
            return store.FindAsync(sku, cancellationToken);
        });
}
