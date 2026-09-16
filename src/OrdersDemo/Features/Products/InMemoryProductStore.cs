namespace OrdersDemo.Features.Products;

internal sealed class InMemoryProductStore : IProductStore
{
    private static readonly TimeSpan SimulatedLatency = TimeSpan.FromMilliseconds(50);

    private static readonly Dictionary<string, Product> Products = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LAPTOP-15"] = new("LAPTOP-15", "15\" Laptop", 1299m),
        ["MOUSE-01"] = new("MOUSE-01", "Wireless Mouse", 29m),
        ["DESK-STD"] = new("DESK-STD", "Standing Desk", 549m),
        ["CHAIR-ERG"] = new("CHAIR-ERG", "Ergonomic Chair", 399m),
    };

    public async Task<Product?> FindAsync(string sku, CancellationToken cancellationToken)
    {
        await Task.Delay(SimulatedLatency, cancellationToken);
        return Products.GetValueOrDefault(sku);
    }
}
