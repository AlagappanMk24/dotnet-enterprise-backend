namespace EnterprisePatterns.Application.Abstractions
{
    /// <summary>Immutable snapshot of business counters.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Memento-style immutable snapshot (record).<br/>
    /// <b>Purpose:</b> Consistent read of counters at one moment.<br/>
    /// <b>Why It Is Used:</b> Callers never see half-updated state or share mutable data.
    /// </remarks>
    public sealed record OrderMetricsSnapshot(long Placed, long Paid, long Shipped, long Cancelled);
}