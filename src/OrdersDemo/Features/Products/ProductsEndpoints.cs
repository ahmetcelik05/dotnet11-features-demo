namespace OrdersDemo.Features.Products;

public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/products/{sku}", async (string sku, IProductCatalog catalog, CancellationToken cancellationToken) =>
            await catalog.GetAsync(sku, cancellationToken) is { } product
                ? Results.Ok(product)
                : Results.NotFound());

        return endpoints;
    }
}
