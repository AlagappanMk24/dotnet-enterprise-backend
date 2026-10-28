using Enterprise.BackgroundJobs.Domain.Enums;

namespace Enterprise.BackgroundJobs.Application.DTOs.Records
{
    public sealed record ClaimedBackgroundJob(Guid Id, BackgroundJobType Type, string Payload, int AttemptCount);
}
