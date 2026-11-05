using EnterprisePatterns.Application.Abstractions;

namespace EnterprisePatterns.Infrastructure.Services;

/// <summary>
/// Lock-free business counters.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Singleton (lifetime managed by the DI container, preferred over a hand-rolled static singleton) +
/// Thread safety via <see cref="Interlocked"/>.<br/>
/// <b>Purpose:</b> Counts placed / paid / shipped / cancelled orders across ALL concurrent requests.<br/>
/// <b>Why It Is Used:</b> Interlocked operations are atomic without locks, so there is no contention and no lost updates.
/// Counters reset on process restart; production systems would export them via System.Diagnostics.Metrics / OpenTelemetry.
/// </remarks>
internal sealed class OrderMetrics : IOrderMetrics
{
    private long _placed;
    private long _paid;
    private long _shipped;
    private long _cancelled;

    public void RecordPlaced() => Interlocked.Increment(ref _placed);

    public void RecordPaid() => Interlocked.Increment(ref _paid);

    public void RecordShipped() => Interlocked.Increment(ref _shipped);

    public void RecordCancelled() => Interlocked.Increment(ref _cancelled);

    public OrderMetricsSnapshot Snapshot() => new(
        Interlocked.Read(ref _placed),
        Interlocked.Read(ref _paid),
        Interlocked.Read(ref _shipped),
        Interlocked.Read(ref _cancelled));
}
