using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace EnterprisePatterns.Infrastructure.Payments;

/// <summary>/// Adapts the vendor client to the Application's <see cref="IPaymentGateway"/> and protects it with Polly./// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Adapter (GoF Structural) + Ports and Adapters + Polly Resilience + Anti-Corruption Layer.<br/>
/// <b>Purpose:</b> Translates currency and amounts to minor units, uses the order id as idempotency key, maps vendor status<br/>
/// strings to <c>PaymentResult</c>, and converts technical failures into <c>PaymentGatewayUnavailableException</c>.<br/>
/// <b>Why It Is Used:</b> Vendor types and Polly never leak into the Application. Replacing the vendor means writing one new adapter.<br/>
/// Registered Transient: it is stateless and depends only on Singletons.
/// </remarks>
internal sealed class PaymentGatewayAdapter(
    ExternalPaymentProviderClient client,
    PaymentResiliencePipeline resilience,
    ILogger<PaymentGatewayAdapter> logger) : IPaymentGateway
{
    public async Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        var externalRequest = new ExternalChargeRequest(
            IdempotencyKey: request.OrderId.ToString("N"),
            AmountInMinorUnits: (long)Math.Round(request.Amount * 100m, MidpointRounding.AwayFromZero),
            CurrencyCode: request.Currency.ToLowerInvariant(),
            SourceToken: request.PaymentToken);

        try
        {
            var response = await resilience.Pipeline
                .ExecuteAsync(async token => await client.CreateChargeAsync(externalRequest, token).ConfigureAwait(false), cancellationToken)
                .ConfigureAwait(false);

            return response.Status == "succeeded" && response.ChargeId is not null
                ? PaymentResult.Success(response.ChargeId)
                : PaymentResult.Declined(response.FailureMessage ?? "Payment was not approved.");
        }
        catch (Exception ex) when (ex is BrokenCircuitException or TimeoutRejectedException or HttpRequestException)
        {
            logger.LogError(ex, "Payment provider unavailable for order {OrderId}", request.OrderId);
            throw new PaymentGatewayUnavailableException("The payment provider is temporarily unavailable. Please retry shortly.", ex);
        }
    }
}