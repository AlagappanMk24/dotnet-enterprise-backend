using Enterprise.BackgroundJobs.Application.DTOs.Records;
using Enterprise.BackgroundJobs.Application.Interfaces;
using Enterprise.BackgroundJobs.Domain.Entities;
using Enterprise.BackgroundJobs.Domain.Enums;
using System.Text.Json;

namespace Enterprise.BackgroundJobs.Infrastructure.Persistence;

public sealed class JobEnqueuer(BackgroundJobsDbContext dbContext, TimeProvider timeProvider) : IJobEnqueuer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<Guid> EnqueueOrderAsync(EnqueueProcessOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty) throw new ArgumentException("OrderId is required.");
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CustomerEmail);

        return SaveAsync(BackgroundJobType.ProcessOrder,
            JsonSerializer.Serialize(new ProcessOrderPayload(request.OrderId, request.CustomerEmail), JsonOptions), cancellationToken);
    }

    public Task<Guid> EnqueueEmailAsync(EnqueueEmailRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.To);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Body);

        return SaveAsync(BackgroundJobType.SendEmail,
            JsonSerializer.Serialize(new SendEmailPayload(request.To, request.Subject, request.Body), JsonOptions), cancellationToken);
    }

    private async Task<Guid> SaveAsync(BackgroundJobType type, string payload, CancellationToken cancellationToken)
    {
        var job = BackgroundJob.Create(type, payload, timeProvider.GetUtcNow());
        dbContext.BackgroundJobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);
        return job.Id;
    }
}