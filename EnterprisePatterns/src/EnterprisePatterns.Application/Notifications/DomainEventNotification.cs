using EnterprisePatterns.Domain.Common;
using MediatR;

namespace EnterprisePatterns.Application.Notifications;

/// <summary>
/// Wraps a pure domain event so MediatR can publish it.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Adapter Pattern (GoF Structural) + Observer (publish/subscribe) + Dependency Inversion.<br/>
/// <b>Purpose:</b> The Domain must stay free of MediatR; this adapter makes its events publishable.<br/>
/// <b>Why It Is Used:</b> Keeps the Domain at zero dependencies while still using MediatR's in-process publish/subscribe.
/// </remarks>
public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : IDomainEvent;