namespace EnterprisePatterns.Application.Exceptions;

/// <summary>Optimistic concurrency conflict.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Optimistic Concurrency Control (error translation).<br/>
/// <b>Purpose:</b> Mapped to HTTP 409 so clients reload and retry.<br/>
/// <b>Why It Is Used:</b> Prevents lost updates without database locks.
/// </remarks>
public sealed class ConcurrencyConflictException(string message, Exception? inner = null) : Exception(message, inner) { }