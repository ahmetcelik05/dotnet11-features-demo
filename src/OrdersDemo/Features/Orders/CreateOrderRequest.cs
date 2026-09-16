using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace OrdersDemo.Features.Orders;

/// <summary>
/// Uses both .NET 11 async validation entry points: an <see cref="AsyncValidationAttribute"/> on a property
/// (<see cref="RegisteredCustomerAttribute"/>) and <see cref="IAsyncValidatableObject"/> on the type.
/// </summary>
public sealed class CreateOrderRequest : IAsyncValidatableObject
{
    [Required]
    [EmailAddress]
    [RegisteredCustomer]
    public string CustomerEmail { get; init; } = string.Empty;

    [MinLength(1)]
    public IReadOnlyList<OrderLine> Lines { get; init; } = [];

    /// <summary>
    /// <see cref="IAsyncValidatableObject"/> still inherits the synchronous <see cref="IValidatableObject.Validate"/>.
    /// This type only validates asynchronously, so reaching the synchronous path is a programming error.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        throw new InvalidOperationException($"Validate {nameof(CreateOrderRequest)} with {nameof(ValidateAsync)}.");

    public async IAsyncEnumerable<ValidationResult> ValidateAsync(
        ValidationContext validationContext,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var inventory = validationContext.GetRequiredService<IInventoryService>();

        foreach (var line in Lines)
        {
            var available = await inventory.GetAvailableQuantityAsync(line.Sku, cancellationToken);
            if (available < line.Quantity)
            {
                yield return new ValidationResult(
                    $"Only {available} unit(s) of '{line.Sku}' are in stock.",
                    [nameof(Lines)]);
            }
        }
    }
}
