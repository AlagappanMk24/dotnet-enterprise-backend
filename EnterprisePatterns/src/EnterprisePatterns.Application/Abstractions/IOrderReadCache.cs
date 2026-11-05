using EnterprisePatterns.Application.DTOs;
using System.Diagnostics.CodeAnalysis;

namespace EnterprisePatterns.Application.Abstractions
{
    /// <summary>Cache of order read models.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Cache-Aside support abstraction + DIP.<br/>
    /// <b>Purpose:</b> Lets event handlers invalidate entries the caching decorator populated.<br/>
    /// <b>Why It Is Used:</b> Bounds staleness; swapping in Redis later only needs a new implementation.
    /// </remarks>
    public interface IOrderReadCache
    {
        bool TryGet(Guid orderId, [NotNullWhen(true)] out OrderDto? order);

        void Set(OrderDto order);

        void Invalidate(Guid orderId);
    }
}