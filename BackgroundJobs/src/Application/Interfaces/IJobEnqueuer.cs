using Enterprise.BackgroundJobs.Application.DTOs.Records;

namespace Enterprise.BackgroundJobs.Application.Interfaces
{
    public interface IJobEnqueuer
    {
        Task<Guid> EnqueueOrderAsync(EnqueueProcessOrderRequest request, CancellationToken cancellationToken);
        Task<Guid> EnqueueEmailAsync(EnqueueEmailRequest request, CancellationToken cancellationToken);
    }
}