namespace SignalRHub.Application.Abstractions;

/// <summary>
/// Remembers which users are connected right now and how many connections each has.
/// The hub fills it on connect and disconnect. Today it is held in memory
/// (InMemoryConnectionTracker); with several servers you would back it with Redis.
/// </summary>
public interface IConnectionTracker
{
    /// <summary>Total open connections across all users.</summary>
    int TotalConnections { get; }

    /// <summary>Records a new connection for a user.</summary>
    void Add(string userId, string connectionId);

    /// <summary>Forgets a connection, for example when the browser tab closes.</summary>
    void Remove(string userId, string connectionId);

    /// <summary>True if the user has at least one open connection.</summary>
    bool IsOnline(string userId);

    /// <summary>How many connections (tabs or devices) the user has open.</summary>
    int GetConnectionCount(string userId);
}
