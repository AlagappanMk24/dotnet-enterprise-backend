using System.ComponentModel.DataAnnotations;

namespace EnterprisePatterns.Infrastructure.Persistence.Outbox;

/// <summary>
/// Tunable settings of the outbox processor.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Options Pattern + fail-fast configuration validation.<br/>
/// <b>Purpose:</b> Strongly typed, validated settings bound from appsettings.json section "Outbox".<br/>
/// <b>Why It Is Used:</b> A mistyped value stops the app at startup instead of failing silently at 3 a.m.
/// </remarks>
public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    [Range(1, 3600)]
    public int PollIntervalSeconds { get; set; } = 2;

    [Range(1, 1000)]
    public int BatchSize { get; set; } = 20;

    [Range(1, 100)]
    public int MaxRetries { get; set; } = 5;
}