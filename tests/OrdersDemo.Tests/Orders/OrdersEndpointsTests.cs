using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using OrdersDemo.Features.Orders;

namespace OrdersDemo.Tests.Orders;

public sealed class OrdersEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Minimal_api_runs_async_validators_and_returns_400()
    {
        var response = await _client.PostAsJsonAsync(
            "/orders",
            new CreateOrderRequest { CustomerEmail = "nobody@example.com", Lines = [new OrderLine { Sku = "MOUSE-01", Quantity = 2 }] },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.Contains(nameof(CreateOrderRequest.CustomerEmail), problem.Errors.Keys);
    }

    [Fact]
    public async Task Valid_order_is_created()
    {
        var response = await _client.PostAsJsonAsync(
            "/orders",
            new CreateOrderRequest { CustomerEmail = "ada@example.com", Lines = [new OrderLine { Sku = "LAPTOP-15", Quantity = 1 }] },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<OrderCreatedResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.StartsWith("ORD-", created.OrderId);
    }
}
