namespace EnterprisePatterns.Application.Abstractions
{
    /// <summary>Business metrics collector.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Singleton (container-managed lifetime) + Interface Segregation.<br/>
    /// <b>Purpose:</b> Thread-safe, process-wide counters.<br/>
    /// <b>Why It Is Used:</b> One shared instance must aggregate counts from all concurrent requests.
    /// </remarks>
    public interface IOrderMetrics
    {
        void RecordPlaced();
        void RecordPaid();
        void RecordShipped();
        void RecordCancelled();
        OrderMetricsSnapshot Snapshot();
    }
}