using EnterprisePatterns.Application.Exceptions;
using FluentValidation;
using MediatR;

namespace EnterprisePatterns.Application.Behaviors;

/// <summary>
/// Runs every FluentValidation validator for a request before its handler executes.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Chain of Responsibility + Fail-Fast + Single Responsibility.<br/>
/// <b>Purpose:</b> Rejects malformed input with a structured error before any business code or I/O runs.<br/>
/// <b>Why It Is Used:</b> Handlers can assume valid input; validation rules live next to the command they guard.
/// Validators run sequentially because a FluentValidation context is not guaranteed to be thread safe.
/// </remarks>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var validatorList = validators.ToArray();
        if (validatorList.Length == 0)
        {
            return await next().ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        foreach (var validator in validatorList)
        {
            var result = await validator.ValidateAsync(context, cancellationToken).ConfigureAwait(false);
            failures.AddRange(result.Errors);
        }

        if (failures.Count > 0)
        {
            var errors = failures
                .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.Distinct().ToArray());

            throw new AppValidationException(errors);
        }

        return await next().ConfigureAwait(false);
    }
}
