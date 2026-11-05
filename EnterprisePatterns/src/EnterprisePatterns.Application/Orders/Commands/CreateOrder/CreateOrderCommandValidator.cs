using FluentValidation;

namespace EnterprisePatterns.Application.Orders.Commands.CreateOrder;

/// <summary>Validates <see cref="CreateOrderCommand"/>.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Validator in the MediatR pipeline (Chain of Responsibility) + Fail-Fast.<br/>
/// <b>Purpose:</b> Rejects bad requests with field-level messages.<br/>
/// <b>Why It Is Used:</b> Protects the domain from garbage and gives clients actionable errors.
/// </remarks>
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().Matches("^[A-Za-z]{3}$").WithMessage("Currency must be a 3-letter code.");
        RuleFor(x => x.CouponCode).MaximumLength(32);
        RuleFor(x => x.ShippingAddress).NotNull().SetValidator(new AddressDtoValidator());
        RuleFor(x => x.Items).NotEmpty().Must(items => items.Count <= 50).WithMessage("An order can contain at most 50 lines.");
        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemDtoValidator());
    }
}