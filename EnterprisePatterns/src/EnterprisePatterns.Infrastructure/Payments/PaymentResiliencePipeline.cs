using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace EnterprisePatterns.Infrastructure.Payments;

/// <summary>/// Holds the shared Polly resilience pipeline for payment calls./// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Polly Resilience (Retry + Circuit Breaker + Timeout) + Singleton (stateful breaker must be shared).<br/>
/// <b>Purpose:</b> Per attempt: 5 s timeout. Transient failures: up to 3 retries with exponential backoff and jitter.<br/>
/// Sustained failures (50% over 30 s, minimum 10 calls): circuit opens for 15 s and fails fast.<br/>
/// <b>Why It Is Used:</b> Stops a slow or dead provider from exhausting threads and causing a cascading outage.<br/>
/// A ResiliencePipeline is thread safe; it MUST be a Singleton, otherwise each request would get a fresh breaker that never trips.
/// </remarks>
public sealed class PaymentResiliencePipeline
{
    public PaymentResiliencePipeline()
    {
        Pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = args => IsTransientFailure(args.Outcome),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(200),
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = args => IsTransientFailure(args.Outcome),
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(15),
            })
            .AddTimeout(TimeSpan.FromSeconds(5))
            .Build();
    }

    public ResiliencePipeline Pipeline { get; }

    /// <summary>Only network-level and timeout failures are worth retrying; business declines are not exceptions at all.</summary>
    private static ValueTask<bool> IsTransientFailure(Outcome<object> outcome) =>
        outcome.Exception is HttpRequestException or TimeoutRejectedException
            ? PredicateResult.True()
            : PredicateResult.False();
}