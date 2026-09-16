namespace OrdersDemo.Features.Products;

/// <summary>The slow source of truth that the catalog caches in front of.</summary>
public interface IProductStore
{
    Task<Product?> FindAsync(string sku, CancellationToken cancellationToken);
}
