using Enterprise.BackgroundJobs.Domain.Enums;

namespace Enterprise.BackgroundJobs.Application.Interfaces
{
    public interface IBackgroundJobProcessor
    {
        Task ProcessAsync(BackgroundJobType type, string payload, CancellationToken cancellationToken);
    }
}