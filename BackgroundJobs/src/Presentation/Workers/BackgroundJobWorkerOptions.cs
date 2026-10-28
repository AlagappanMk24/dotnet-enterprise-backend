namespace Enterprise.BackgroundJobs.Api.Workers
{
    public sealed class BackgroundJobWorkerOptions
    {
        public const string SectionName = "BackgroundJobWorker";
        public int PollIntervalSeconds { get; init; } = 3;
        public int BatchSize { get; init; } = 10;
        public int MaxAttempts { get; init; } = 8;
        public int LeaseDurationSeconds { get; init; } = 120;
        public int IdleDelayMilliseconds { get; init; } = 500;
    }
}
