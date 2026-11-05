using FluentValidation;
using EnterprisePatterns.Application.DTOs;

namespace EnterprisePatterns.Application.Orders.Commands.CreateOrder;

/// <summary>Validates an address DTO.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Reusable Specification-style validator (Composition over duplication).<br/>
/// <b>Purpose:</b> Shared by any command carrying an address.<br/>
/// <b>Why It Is Used:</b> One rule set, many consumers (DRY).
/// </remarks>
public sealed class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}