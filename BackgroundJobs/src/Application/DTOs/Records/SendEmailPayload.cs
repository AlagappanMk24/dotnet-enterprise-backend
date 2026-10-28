namespace Enterprise.BackgroundJobs.Application.DTOs.Records
{
    public sealed record SendEmailPayload(string To, string Subject, string Body);
}