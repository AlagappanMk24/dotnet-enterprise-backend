namespace EnterprisePatterns.Api.Contracts;

/// <summary>Body of the pay endpoint.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Request DTO (API contract separate from the internal command).<br/>
/// <b>Purpose:</b> The order id comes from the route; only the payment token comes from the body.<br/>
/// <b>Why It Is Used:</b> A client can never override the route id through the body (prevents ambiguity and tampering).
/// </remarks>
public sealed record PayOrderRequest(string PaymentToken);

/// <summary>Body of the ship endpoint.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Request DTO.<br/>
/// <b>Purpose:</b> Carries the carrier tracking number.<br/>
/// <b>Why It Is Used:</b> Keeps the HTTP contract stable even if the command evolves.
/// </remarks>
public sealed record ShipOrderRequest(string TrackingNumber);

/// <summary>Body of the cancel endpoint.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Request DTO.<br/>
/// <b>Purpose:</b> Carries the cancellation reason.<br/>
/// <b>Why It Is Used:</b> Same reasoning as the other request records.
/// </remarks>
public sealed record CancelOrderRequest(string Reason);
