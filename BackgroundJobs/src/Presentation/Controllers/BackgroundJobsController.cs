using Enterprise.BackgroundJobs.Application.DTOs.Records;
using Enterprise.BackgroundJobs.Application.Interfaces;
using Enterprise.BackgroundJobs.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.BackgroundJobs.Api.Controllers;

[ApiController]
[Route("api/background-jobs")]
public sealed class BackgroundJobsController(
    IJobEnqueuer jobEnqueuer,
    BackgroundJobsDbContext dbContext) : ControllerBase
{
    [HttpPost("orders")]
    public async Task<IActionResult> EnqueueOrder(EnqueueProcessOrderRequest request, CancellationToken cancellationToken)
    {
        var jobId = await jobEnqueuer.EnqueueOrderAsync(request, cancellationToken);
        return Accepted(new EnqueueJobResponse(jobId, "Pending"));
    }

    [HttpPost("emails")]
    public async Task<IActionResult> EnqueueEmail(EnqueueEmailRequest request, CancellationToken cancellationToken)
    {
        var jobId = await jobEnqueuer.EnqueueEmailAsync(request, cancellationToken);
        return Accepted(new EnqueueJobResponse(jobId, "Pending"));
    }

    [HttpGet("{jobId:guid}")]
    public async Task<IActionResult> GetJobStatus(Guid jobId, CancellationToken cancellationToken)
    {
        var jobDetails = await dbContext.BackgroundJobs
            .AsNoTracking()
            .Where(job => job.Id == jobId)
            .Select(job => new
            {
                job.Id,
                Type = job.Type.ToString(),
                Status = job.Status.ToString(),
                job.AttemptCount,
                job.CreatedAtUtc,
                job.NextAttemptAtUtc,
                job.LockedUntilUtc,
                job.CompletedAtUtc,
                job.LastError
            })
            .SingleOrDefaultAsync(cancellationToken);

        return jobDetails is null ? NotFound() : Ok(jobDetails);
    }
}