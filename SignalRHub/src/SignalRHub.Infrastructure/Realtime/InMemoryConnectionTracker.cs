using System.Collections.Concurrent;
using SignalRHub.Application.Abstractions;

namespace SignalRHub.Infrastructure.Realtime;

/// <summary>
/// Keeps "who is connected" in the server's memory. Thread-safe, because many
/// connections open and close at the same time.
/// Limitation: each server instance has its own copy. With several servers,
/// replace this with a Redis-backed implementation of IConnectionTracker.
/// </summary>
public sealed class InMemoryConnectionTracker : IConnectionTracker
{
    // userId -> set of that user's connection ids. The byte value is unused;
    // the dictionary is just used as a thread-safe set.
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _connections = new();

    /// <inheritdoc />
    public int TotalConnections => _connections.Values.Sum(c => c.Count);

    /// <inheritdoc />
    public void Add(string userId, string connectionId) =>
        _connections.GetOrAdd(userId, _ => new()).TryAdd(connectionId, 0);

    /// <inheritdoc />
    public void Remove(string userId, string connectionId)
    {
        if (!_connections.TryGetValue(userId, out var set)) return;

        set.TryRemove(connectionId, out _);

        // Drop the user entry when their last connection closes, so the dictionary does not grow forever.
        // The key/value overload only removes it if nobody added a connection in the meantime.
        if (set.IsEmpty)
            _connections.TryRemove(new KeyValuePair<string, ConcurrentDictionary<string, byte>>(userId, set));
    }

    /// <inheritdoc />
    public bool IsOnline(string userId) => GetConnectionCount(userId) > 0;

    /// <inheritdoc />
    public int GetConnectionCount(string userId) =>
        _connections.TryGetValue(userId, out var set) ? set.Count : 0;
}
