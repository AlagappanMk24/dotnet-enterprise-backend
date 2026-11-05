namespace EnterprisePatterns.Domain.Common;

/// <summary>
/// Non-generic view of "an object that collects domain events".
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Interface Segregation Principle (ISP) + Dependency Inversion Principle (DIP).<br/>
/// <b>Purpose:</b> Lets Infrastructure harvest events from ANY aggregate without knowing its concrete type.<br/>
/// <b>Why It Is Used:</b> The DbContext can scan the change tracker for this single interface instead of
/// hard-coding every aggregate, so adding a new aggregate needs zero changes in persistence plumbing (Open/Closed).
/// </remarks>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}