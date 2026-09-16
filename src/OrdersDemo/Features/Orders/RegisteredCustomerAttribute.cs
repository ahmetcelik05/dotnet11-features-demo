using System.ComponentModel.DataAnnotations;

namespace OrdersDemo.Features.Orders;

/// <summary>
/// An asynchronous validation rule that needs I/O: the e-mail must exist in the customer directory.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RegisteredCustomerAttribute : AsyncValidationAttribute
{
    /// <summary>
    /// The synchronous overload is abstract, so every async attribute has to decide what to do here.
    /// We follow the framework guidance and throw, so accidental synchronous validation fails loudly.
    /// </summary>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        throw new InvalidOperationException(
            $"{nameof(RegisteredCustomerAttribute)} only supports asynchronous validation. Use Validator.ValidateObjectAsync.");

    protected override async Task<ValidationResult?> IsValidAsync(
        object? value,
        ValidationContext validationContext,
        CancellationToken cancellationToken)
    {
        if (value is not string email || string.IsNullOrWhiteSpace(email))
        {
            return ValidationResult.Success; // [Required] owns the empty-value case
        }

        var directory = validationContext.GetRequiredService<ICustomerDirectory>();

        return await directory.ExistsAsync(email, cancellationToken)
            ? ValidationResult.Success
            : new ValidationResult($"'{email}' is not a registered customer.", [validationContext.MemberName!]);
    }
}
