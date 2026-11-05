namespace EnterprisePatterns.Domain.Exceptions;

/// <summary>
/// Raised when a business invariant is violated.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Fail-Fast + Domain Exception (DDD) + Single Responsibility.<br/>
/// <b>Purpose:</b> Distinguishes "the business rule forbids this" from technical failures (timeouts, null refs).<br/>
/// <b>Why It Is Used:</b> The API layer maps this type to a 4xx response, while unexpected exceptions become 500s,
/// so clients and on-call engineers can tell the two apart.
/// </remarks>
public class DomainException(string message) : Exception(message) { }
