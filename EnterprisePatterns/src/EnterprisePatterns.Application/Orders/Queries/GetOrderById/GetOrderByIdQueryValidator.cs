using FluentValidation;

namespace EnterprisePatterns.Application.Orders.Queries.GetOrderById;

/// <summary>Validates <see cref="GetOrderByIdQuery"/>.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Fluent Validation in the pipeline.<br/>
/// <b>Purpose:</b> Rejects an empty id early.<br/>
/// <b>Why It Is Used:</b> Avoids a pointless database round trip.
/// </remarks>
public sealed class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}