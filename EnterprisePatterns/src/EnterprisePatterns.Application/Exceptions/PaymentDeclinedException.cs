namespace EnterprisePatterns.Application.Exceptions;

/// <summary>The payment provider rejected the payment.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Custom exception (business failure, not technical).<br/>
/// <b>Purpose:</b> Mapped to HTTP 402 Payment Required.<br/>
/// <b>Why It Is Used:</b> A decline is an expected outcome and must not be logged as a system error.
/// </remarks>
public sealed class PaymentDeclinedException(Guid orderId, string reason)
    : Exception($"Payment for order {orderId} was declined: {reason}")
{ }