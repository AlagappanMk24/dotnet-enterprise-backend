namespace EnterprisePatterns.Application.Common;

/// <summary>Generic page of results with paging metadata.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Data Transfer Object (immutable record) + DRY generic type.<br/>
/// <b>Purpose:</b> Uniform paging envelope for every list endpoint.<br/>
/// <b>Why It Is Used:</b> Prevents unbounded result sets (memory, latency) and gives clients consistent paging fields.
/// </remarks>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
