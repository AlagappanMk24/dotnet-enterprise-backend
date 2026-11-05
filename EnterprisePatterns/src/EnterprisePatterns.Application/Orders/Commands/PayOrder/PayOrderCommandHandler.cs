using FluentValidation;

namespace EnterprisePatterns.Application.Orders.Commands.PayOrder;

/// <summary>Validates <see cref="PayOrderCommand"/>.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Fluent Validation in the pipeline.<br/>
/// <b>Purpose:</b> Ensures an id and token are present.<br/>
/// <b>Why It Is Used:</b> Fail fast before any network call to the payment provider.
/// </remarks>
public sealed class PayOrderCommandValidator : AbstractValidator<PayOrderCommand>
{
    public PayOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.PaymentToken).NotEmpty().MaximumLength(200);
    }
}