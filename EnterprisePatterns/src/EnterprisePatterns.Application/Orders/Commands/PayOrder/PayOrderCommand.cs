using EnterprisePatterns.Application.Common;
using EnterprisePatterns.Application.DTOs;

namespace EnterprisePatterns.Application.Orders.Commands.PayOrder;

/// <summary>Pays for a placed order.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Command (GoF) + CQRS write model.<br/>
/// <b>Purpose:</b> Carries the order id and a tokenised payment method (never raw card data).<br/>
/// <b>Why It Is Used:</b> Tokenisation keeps the API out of PCI-DSS scope.
/// </remarks>
public sealed record PayOrderCommand(Guid OrderId, string PaymentToken) : ICommand<OrderDto>;