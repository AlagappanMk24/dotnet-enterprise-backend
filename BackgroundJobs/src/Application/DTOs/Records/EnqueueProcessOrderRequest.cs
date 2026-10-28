namespace Enterprise.BackgroundJobs.Application.DTOs.Records
{
    public sealed record EnqueueProcessOrderRequest(Guid OrderId, string CustomerEmail);
}