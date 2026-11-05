namespace EnterprisePatterns.Application.Abstractions
{
    /// <summary>Sends messages to customers.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Port (DIP) + Strategy-ready abstraction (email, SMS, push).<br/>
    /// <b>Purpose:</b> Event handlers notify customers without knowing the channel.<br/>
    /// <b>Why It Is Used:</b> New channels are added by registering another implementation.
    /// </remarks>
    public interface INotificationSender
    {
        Task SendAsync(Guid customerId, string subject, string message, CancellationToken cancellationToken);
    }
}