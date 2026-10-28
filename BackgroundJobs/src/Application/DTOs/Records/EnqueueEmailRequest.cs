namespace Enterprise.BackgroundJobs.Application.DTOs.Records
{
    public sealed record EnqueueEmailRequest(string To, string Subject, string Body);
}