using Enterprise.BackgroundJobs.Application.DTOs.Records;
using Enterprise.BackgroundJobs.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Enterprise.BackgroundJobs.Api.Workers;

public sealed class BackgroundJobWorker(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<BackgroundJobWorkerOptions> workerOptions,
    ILogger<BackgroundJobWorker> logger) : BackgroundService
{
    private readonly BackgroundJobWorkerOptions _workerOptions = workerOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Background worker started. BatchSize={BatchSize}", _workerOptions.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                IReadOnlyList<ClaimedBackgroundJob> claimedJobs;

                await using (var scope = serviceScopeFactory.CreateAsyncScope())
                {
                    var backgroundJobRepository = scope.ServiceProvider.GetRequiredService<IBackgroundJobRepository>();

                    claimedJobs = await backgroundJobRepository.ClaimBatchAsync(
                        _workerOptions.BatchSize,
                        TimeSpan.FromSeconds(_workerOptions.LeaseDurationSeconds),
                        stoppingToken);
                }

                if (claimedJobs.Count == 0)
                {
                    await Task.Delay(_workerOptions.IdleDelayMilliseconds, stoppingToken);
                    continue;
                }

                foreach (var job in claimedJobs)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    await ProcessJobAsync(job, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Worker loop failed unexpectedly.");

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_workerOptions.PollIntervalSeconds), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        logger.LogInformation("Background worker stopped.");
    }

    private async Task ProcessJobAsync(ClaimedBackgroundJob job, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var jobProcessor = scope.ServiceProvider.GetRequiredService<IBackgroundJobProcessor>();

            await jobProcessor.ProcessAsync(job.Type, job.Payload, cancellationToken);

            await scope.ServiceProvider.GetRequiredService<IBackgroundJobRepository>()
                .MarkCompletedAsync(job.Id, cancellationToken);

            logger.LogInformation("Job {JobId} completed successfully. Type={Type}, Attempt={Attempt}",
                job.Id, job.Type, job.AttemptCount);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Shutdown interrupted job {JobId}; lease-based recovery will retry it.", job.Id);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Job {JobId} failed on attempt {Attempt}", job.Id, job.AttemptCount);

            try
            {
                await using var scope = serviceScopeFactory.CreateAsyncScope();

                await scope.ServiceProvider.GetRequiredService<IBackgroundJobRepository>()
                    .ScheduleRetryOrFailAsync(job.Id, job.AttemptCount, _workerOptions.MaxAttempts, exception.ToString(), cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception persistenceException)
            {
                logger.LogCritical(persistenceException, "Could not schedule retry for job {JobId}; lease recovery will apply.", job.Id);
            }
        }
    }
}