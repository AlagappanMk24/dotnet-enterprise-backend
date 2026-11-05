namespace EnterprisePatterns.Application.Exceptions;

/// <summary>The payment provider is unreachable (after retries / circuit open).</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Custom exception (technical failure translated to a stable contract).<br/>
/// <b>Purpose:</b> Mapped to HTTP 503 so clients know it is safe to retry later.<br/>
/// <b>Why It Is Used:</b> Hides Polly and HTTP exception types from the rest of the system.
/// </remarks>
public sealed class PaymentGatewayUnavailableException(string message, Exception? inner = null) : Exception(message, inner) { }