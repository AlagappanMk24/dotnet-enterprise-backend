using FluentValidation;
using EnterprisePatterns.Application.DTOs;

namespace EnterprisePatterns.Application.Orders.Commands.CreateOrder;

/// <summary>Validates one input order line.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Reusable validator + Single Responsibility.<br/>
/// <b>Purpose:</b> Checks SKU, name, price and quantity ranges.<br/>
/// <b>Why It Is Used:</b> Cheap syntactic checks happen before domain rules run.
/// </remarks>
public sealed class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
        RuleFor(x => x.Quantity).InclusiveBetween(1, 1000);
    }
}