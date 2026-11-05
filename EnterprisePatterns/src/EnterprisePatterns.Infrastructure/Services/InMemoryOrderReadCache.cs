using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.DTOs;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;

namespace EnterprisePatterns.Infrastructure.Services;

/// <summary>
/// IMemoryCache-backed order read cache.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Cache-Aside support + Singleton + Adapter over IMemoryCache.<br/>
/// <b>Purpose:</b> Stores immutable OrderDto records for 30 seconds.<br/>
/// <b>Why It Is Used:</b> IMemoryCache is thread safe and the DTOs are immutable, so no locking is needed.
/// For multiple API instances, swap this class for a Redis implementation (the interface stays).
/// </remarks>
internal sealed class InMemoryOrderReadCache(IMemoryCache cache) : IOrderReadCache
{
    private static readonly TimeSpan TimeToLive = TimeSpan.FromSeconds(30);

    public bool TryGet(Guid orderId, [NotNullWhen(true)] out OrderDto? order) => cache.TryGetValue(Key(orderId), out order);

    public void Set(OrderDto order) => cache.Set(Key(order.Id), order, TimeToLive);

    public void Invalidate(Guid orderId) => cache.Remove(Key(orderId));

    private static string Key(Guid orderId) => $"order:{orderId:N}";
}