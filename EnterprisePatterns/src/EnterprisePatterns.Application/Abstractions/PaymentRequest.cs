namespace EnterprisePatterns.Application.Abstractions
{
    /// <summary>Request to charge a customer.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> DTO owned by the Application (the "Target" interface of the Adapter).<br/>
    /// <b>Purpose:</b> Provider-neutral payment request.<br/>
    /// <b>Why It Is Used:</b> Switching payment vendors never changes use cases.
    /// </remarks>
    public sealed record PaymentRequest(Guid OrderId, decimal Amount, string Currency, string PaymentToken);
}