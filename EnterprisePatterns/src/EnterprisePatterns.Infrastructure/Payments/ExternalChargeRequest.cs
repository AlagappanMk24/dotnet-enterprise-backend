namespace EnterprisePatterns.Infrastructure.Payments;

/// <summary>Charge request in the (simulated) third-party provider's own vocabulary.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Adaptee data contract (the foreign model the Adapter translates from/to).<br/>
/// <b>Purpose:</b> Mirrors how real SDKs use minor units, idempotency keys and source tokens.<br/>
/// <b>Why It Is Used:</b> Shows why an Adapter is needed: this shape must never leak into the Application layer.
/// </remarks>
public sealed record ExternalChargeRequest(string IdempotencyKey, long AmountInMinorUnits, string CurrencyCode, string SourceToken);