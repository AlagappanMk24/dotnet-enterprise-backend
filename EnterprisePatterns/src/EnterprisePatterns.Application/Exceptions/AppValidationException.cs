namespace EnterprisePatterns.Application.Exceptions;

/// <summary>Input failed FluentValidation rules.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Custom exception carrying structured data.<br/>
/// <b>Purpose:</b> Mapped to HTTP 400 with a field-by-field error dictionary.<br/>
/// <b>Why It Is Used:</b> Application layer owns its own validation exception, so callers do not depend on FluentValidation types.
/// </remarks>
public sealed class AppValidationException(IDictionary<string, string[]> errors) : Exception("One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}