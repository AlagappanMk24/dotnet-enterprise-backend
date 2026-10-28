namespace Enterprise.BackgroundJobs.Application.Interfaces
{
    public interface IExternalOrderClient
    {
        Task NotifyOrderProcessedAsync(Guid orderId, CancellationToken cancellationToken);
    }
}