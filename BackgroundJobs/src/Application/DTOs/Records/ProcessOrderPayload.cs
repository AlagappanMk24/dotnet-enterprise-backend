namespace Enterprise.BackgroundJobs.Application.DTOs.Records
{
    public sealed record ProcessOrderPayload(Guid OrderId, string CustomerEmail);
}