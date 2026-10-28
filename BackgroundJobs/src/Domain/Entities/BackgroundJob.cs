using Enterprise.BackgroundJobs.Domain.Enums;

namespace Enterprise.BackgroundJobs.Domain.Entities
{
    public sealed class BackgroundJob
    {
        private BackgroundJob() { }

        public Guid Id { get; private set; }
        public BackgroundJobType Type { get; private set; }
        public string Payload { get; private set; } = "{}";
        public BackgroundJobStatus Status { get; private set; }
        public int AttemptCount { get; private set; }
        public DateTimeOffset CreatedAtUtc { get; private set; }
        public DateTimeOffset NextAttemptAtUtc { get; private set; }
        public DateTimeOffset? LockedUntilUtc { get; private set; }
        public DateTimeOffset? CompletedAtUtc { get; private set; }
        public string? LastError { get; private set; }

        public static BackgroundJob Create(BackgroundJobType type, string payload, DateTimeOffset now)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(payload);
            return new BackgroundJob
            {
                Id = Guid.NewGuid(),
                Type = type,
                Payload = payload,
                Status = BackgroundJobStatus.Pending,
                CreatedAtUtc = now,
                NextAttemptAtUtc = now
            };
        }
    }
}