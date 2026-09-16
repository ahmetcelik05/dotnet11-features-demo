using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using OrdersDemo.Features.Orders;

namespace OrdersDemo.Tests.Orders;

public sealed class AsyncValidationTests
{
    private static readonly IServiceProvider Services = new ServiceCollection()
        .AddSingleton<ICustomerDirectory, InMemoryCustomerDirectory>()
        .AddSingleton<IInventoryService, InMemoryInventoryService>()
        .BuildServiceProvider();

    private static ValidationContext ContextFor(object model) => new(model, Services, items: null);

    [Fact]
    public async Task Unknown_customer_fails_on_the_async_attribute()
    {
        var request = new CreateOrderRequest
        {
            CustomerEmail = "nobody@example.com",
            Lines = [new OrderLine { Sku = "LAPTOP-15", Quantity = 1 }],
        };
        var results = new List<ValidationResult>();

        var isValid = await Validator.TryValidateObjectAsync(
            request, ContextFor(request), results, validateAllProperties: true, TestContext.Current.CancellationToken);

        Assert.False(isValid);
        var failure = Assert.Single(results);
        Assert.Contains("not a registered customer", failure.ErrorMessage);
        Assert.Equal([nameof(CreateOrderRequest.CustomerEmail)], failure.MemberNames);
    }

    [Fact]
    public async Task Object_level_async_rule_runs_only_after_properties_pass()
    {
        var request = new CreateOrderRequest
        {
            CustomerEmail = "ada@example.com",
            Lines = [new OrderLine { Sku = "MOUSE-01", Quantity = 2 }], // out of stock
        };
        var results = new List<ValidationResult>();

        var isValid = await Validator.TryValidateObjectAsync(
            request, ContextFor(request), results, validateAllProperties: true, TestContext.Current.CancellationToken);

        Assert.False(isValid);
        var failure = Assert.Single(results);
        Assert.Equal("Only 0 unit(s) of 'MOUSE-01' are in stock.", failure.ErrorMessage);
    }

    [Fact]
    public async Task Valid_request_passes()
    {
        var request = new CreateOrderRequest
        {
            CustomerEmail = "ada@example.com",
            Lines = [new OrderLine { Sku = "LAPTOP-15", Quantity = 2 }],
        };

        await Validator.ValidateObjectAsync(
            request, ContextFor(request), validateAllProperties: true, TestContext.Current.CancellationToken);
    }

    [Fact]
    public void Synchronous_validator_throws_on_async_only_attribute()
    {
        var request = new CreateOrderRequest { CustomerEmail = "ada@example.com" };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            Validator.TryValidateObject(request, ContextFor(request), new List<ValidationResult>(), validateAllProperties: true));

        Assert.Contains("only supports asynchronous validation", exception.Message);
    }

    [Fact]
    public async Task Cancellation_flows_into_async_rules()
    {
        var request = new CreateOrderRequest { CustomerEmail = "ada@example.com" };
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Validator.ValidateObjectAsync(request, ContextFor(request), validateAllProperties: true, cancellation.Token));
    }
}
