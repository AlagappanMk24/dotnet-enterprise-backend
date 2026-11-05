using FluentValidation;

namespace EnterprisePatterns.Application.Orders.Commands.ShipOrder;

/// <summary>Validates <see cref="ShipOrderCommand"/>.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Fluent Validation in the pipeline.<br/>
/// <b>Purpose:</b> Requires id and a bounded tracking number.<br/>
/// <b>Why It Is Used:</b> Fail-fast input hygiene.
/// </remarks>
public sealed class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
{
    public ShipOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.TrackingNumber).NotEmpty().MaximumLength(64);
    }
}