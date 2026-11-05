using MediatR;

namespace EnterprisePatterns.Application.Common;

/// <summary>Non-generic marker so pipeline behaviors can detect "this request mutates state".</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Marker Interface + Interface Segregation Principle.<br/>
/// <b>Purpose:</b> Lets the UnitOfWorkBehavior commit only for commands, never for queries.<br/>
/// <b>Why It Is Used:</b> Queries stay side-effect free and cheap; commits are never forgotten for commands.
/// </remarks>
public interface ICommandBase { }

/// <summary>A request that changes system state (write side of CQRS).</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Command Pattern (GoF Behavioral) + CQRS.<br/>
/// <b>Purpose:</b> Encapsulates "do this" as an object that can be validated, logged, queued and handled.<br/>
/// <b>Why It Is Used:</b> Separates intent from execution, enabling cross-cutting pipeline behaviors.
/// </remarks>
public interface ICommand<out TResponse> : IRequest<TResponse>, ICommandBase { }

/// <summary>A request that only reads state (read side of CQRS).</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> CQRS (Command Query Responsibility Segregation).<br/>
/// <b>Purpose:</b> Marks read-only use cases so they can use optimized, cached read paths.<br/>
/// <b>Why It Is Used:</b> Reads and writes scale and evolve independently in production.
/// </remarks>
public interface IQuery<out TResponse> : IRequest<TResponse> { }
