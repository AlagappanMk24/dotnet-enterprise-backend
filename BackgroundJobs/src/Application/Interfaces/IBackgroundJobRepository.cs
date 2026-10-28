using Enterprise.BackgroundJobs.Application.DTOs.Records;

namespace Enterprise.BackgroundJobs.Application.Interfaces
{
    public interface IBackgroundJobRepository
    {
        Task<IReadOnlyList<ClaimedBackgroundJob>> ClaimBatchAsync(int batchSize, TimeSpan lease, CancellationToken cancellationToken);
        Task MarkCompletedAsync(Guid id, CancellationToken cancellationToken);
        Task ScheduleRetryOrFailAsync(Guid id, int attempts, int maxAttempts, string error, CancellationToken cancellationToken);
    }
}