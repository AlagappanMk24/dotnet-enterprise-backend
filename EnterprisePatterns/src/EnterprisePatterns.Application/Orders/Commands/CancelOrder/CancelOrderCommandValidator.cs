using FluentValidation;

namespace EnterprisePatterns.Application.Orders.Commands.CancelOrder;

/// <summary>Validates <see cref="CancelOrderCommand"/>.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Fluent Validation in the pipeline.<br/>
/// <b>Purpose:</b> Bounds the reason text.<br/>
/// <b>Why It Is Used:</b> Prevents oversized payloads reaching the database.
/// </remarks>
public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}