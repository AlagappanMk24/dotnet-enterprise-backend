namespace EnterprisePatterns.Infrastructure.Payments;

/// <summary>Charge response in the provider's vocabulary.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Adaptee data contract.<br/>
/// <b>Purpose:</b> Status strings and charge ids as the vendor returns them.<br/>
/// <b>Why It Is Used:</b> Translated into <c>PaymentResult</c> by the adapter.
/// </remarks>
public sealed record ExternalChargeResponse(string Status, string? ChargeId, string? FailureMessage);