using FluentValidation;

namespace EnterprisePatterns.Application.Orders.Queries.ListOrders;

/// <summary>Validates <see cref="ListOrdersQuery"/>.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Fluent Validation in the pipeline.<br/>
/// <b>Purpose:</b> Enforces sane paging limits.<br/>
/// <b>Why It Is Used:</b> Protects the database from expensive queries.
/// </remarks>
public sealed class ListOrdersQueryValidator : AbstractValidator<ListOrdersQuery>
{
    public ListOrdersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
    }
}