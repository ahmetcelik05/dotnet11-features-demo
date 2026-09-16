namespace OrdersDemo.Features.Products;

public interface IProductCatalog
{
    Task<Product?> GetAsync(string sku, CancellationToken cancellationToken);
}
