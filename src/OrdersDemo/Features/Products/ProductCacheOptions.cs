namespace OrdersDemo.Features.Products;

/// <summary>Bound from the <c>ProductCache</c> configuration section.</summary>
public sealed class ProductCacheOptions
{
    public const string SectionName = "ProductCache";

    /// <summary>Maximum number of cached products (each entry has size 1). Small on purpose so evictions show up.</summary>
    public long SizeLimit { get; init; } = 3;

    /// <summary>
    /// Fraction of entries removed when the limit is exceeded. The framework default (0.05) rounds to zero entries
    /// for a cache this small, so no eviction would ever be observed.
    /// </summary>
    public double CompactionPercentage { get; init; } = 0.5;
}
