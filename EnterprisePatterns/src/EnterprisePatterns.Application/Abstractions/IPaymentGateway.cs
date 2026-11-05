namespace EnterprisePatterns.Application.Abstractions;

/// <summary>Payment capability required by the Application.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Port in Ports-and-Adapters + Dependency Inversion Principle.<br/>
/// <b>Purpose:</b> The only payment API use cases see.<br/>
/// <b>Why It Is Used:</b> Infrastructure adapts any third-party SDK to this stable interface.
/// </remarks>
public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken);
}