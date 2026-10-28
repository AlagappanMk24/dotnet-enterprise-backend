namespace Enterprise.BackgroundJobs.Application.DTOs.Records
{
    public sealed record EnqueueJobResponse(Guid JobId, string Status);
}