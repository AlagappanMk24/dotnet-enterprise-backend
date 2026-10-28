using Enterprise.BackgroundJobs.Application.DTOs.Records;
using Enterprise.BackgroundJobs.Application.Interfaces;
using Enterprise.BackgroundJobs.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.BackgroundJobs.Infrastructure.Persistence
{
    public sealed class BackgroundJobRepository(
      BackgroundJobsDbContext dbContext,
      TimeProvider timeProvider) : IBackgroundJobRepository
    {

        public async Task<IReadOnlyList<ClaimedBackgroundJob>> ClaimBatchAsync(
            int batchSize,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken)
        {
            if (batchSize <= 0)
            {
                return [];
            }

            var currentUtcTime = timeProvider.GetUtcNow().UtcDateTime;
            var leaseExpirationTime = currentUtcTime.Add(leaseDuration);

            var pendingStatus = (int)BackgroundJobStatus.Pending;
            var processingStatus = (int)BackgroundJobStatus.Processing;

            var sql = """
        ;WITH next_jobs AS
        (
            SELECT TOP (@BatchSize) *
            FROM dbo.BackgroundJobs WITH (UPDLOCK, READPAST, ROWLOCK)
            WHERE
                (
                    Status = @PendingStatus
                    AND NextAttemptAtUtc <= @CurrentUtcTime
                )
                OR
                (
                    Status = @ProcessingStatus
                    AND LockedUntilUtc IS NOT NULL
                    AND LockedUntilUtc <= @CurrentUtcTime
                )
            ORDER BY CreatedAtUtc
        )
        UPDATE next_jobs
        SET
            Status = @ProcessingStatus,
            LockedUntilUtc = @LeaseExpirationTime,
            AttemptCount = AttemptCount + 1
        OUTPUT
            inserted.Id,
            inserted.Type,
            inserted.Payload,
            inserted.AttemptCount;
        """;

            return await dbContext.Database
                .SqlQueryRaw<ClaimedBackgroundJob>(
                    sql,
                    new Microsoft.Data.SqlClient.SqlParameter(
                        "@BatchSize", batchSize),
                    new Microsoft.Data.SqlClient.SqlParameter(
                        "@PendingStatus", pendingStatus),
                    new Microsoft.Data.SqlClient.SqlParameter(
                        "@ProcessingStatus", processingStatus),
                    new Microsoft.Data.SqlClient.SqlParameter(
                        "@CurrentUtcTime", currentUtcTime),
                    new Microsoft.Data.SqlClient.SqlParameter(
                        "@LeaseExpirationTime", leaseExpirationTime))
                .ToListAsync(cancellationToken);
        }


        public async Task MarkCompletedAsync(Guid jobId, CancellationToken cancellationToken)
        {
            var completedStatus = (int)BackgroundJobStatus.Completed;
            var completionTime = timeProvider.GetUtcNow().UtcDateTime;

            await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE dbo.BackgroundJobs 
            SET Status={completedStatus},
                CompletedAtUtc={completionTime}, 
                LockedUntilUtc=NULL, 
                LastError=NULL
            WHERE Id={jobId}", cancellationToken);
        }

        public async Task ScheduleRetryOrFailAsync(
             Guid jobId,
             int currentAttemptCount,
             int maxAttempts,
             string errorMessage,
             CancellationToken cancellationToken)
        {
            var isTerminalFailure = currentAttemptCount >= maxAttempts;
            var clampedAttempts = Math.Clamp(currentAttemptCount, 0, 10);

            var backoffDelaySeconds = Math.Min(900, Math.Pow(2, clampedAttempts)) + Random.Shared.NextDouble();
            var nextAttemptTime = timeProvider.GetUtcNow().AddSeconds(backoffDelaySeconds).UtcDateTime;

            var truncatedErrorMessage = errorMessage.Length > 3900 ? errorMessage[..3900] : errorMessage;
            var targetStatus = (int)(isTerminalFailure ? BackgroundJobStatus.Failed : BackgroundJobStatus.Pending);

            await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE dbo.BackgroundJobs 
            SET Status={targetStatus},
                NextAttemptAtUtc={nextAttemptTime}, 
                LockedUntilUtc=NULL,
                LastError={truncatedErrorMessage}
            WHERE Id={jobId}", cancellationToken);
        }
    }
}