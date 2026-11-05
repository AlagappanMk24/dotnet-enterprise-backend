using System.Collections.Concurrent;

namespace EnterprisePatterns.Infrastructure.Payments;

/// <summary>/// Stand-in for a real vendor SDK (Stripe, Razorpay...). It deliberately has an incompatible API./// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Adaptee (the class being adapted) + Singleton lifetime (holds shared attempt counters).<br/>
/// <b>Purpose:</b> Lets you exercise success, decline, transient failure (Polly retry) and outage (circuit breaker):<br/>
/// tokens <c>tok_decline</c>, <c>tok_flaky</c> (fails twice, then succeeds) and <c>tok_down</c>; anything else succeeds.<br/>
/// <b>Why It Is Used:</b> Makes resilience behaviour demonstrable without external accounts. The counter dictionary is a<br/>
/// ConcurrentDictionary, so concurrent requests are safe.
/// </remarks>
public sealed class ExternalPaymentProviderClient
{
    private readonly ConcurrentDictionary<string, int> _attempts = new();

    public async Task<ExternalChargeResponse> CreateChargeAsync(ExternalChargeRequest request, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(25), cancellationToken).ConfigureAwait(false);

        switch (request.SourceToken)
        {
            case "tok_decline":
                return new ExternalChargeResponse("failed", null, "Card declined by issuer.");

            case "tok_down":
                throw new HttpRequestException("Simulated provider outage.");

            case "tok_flaky":
                var attempt = _attempts.AddOrUpdate(request.IdempotencyKey, 1, static (_, current) => current + 1);
                if (attempt <= 2)
                {
                    throw new HttpRequestException($"Simulated transient network error (attempt {attempt}).");
                }

                break;
        }

        return new ExternalChargeResponse("succeeded", $"ch_{Guid.NewGuid():N}", null);
    }
}