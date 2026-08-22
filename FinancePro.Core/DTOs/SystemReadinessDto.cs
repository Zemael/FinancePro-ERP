namespace FinancePro.Core.DTOs;

public sealed class SystemReadinessDto
{
    public DateTime CheckedAt { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public int Passed { get; set; }
    public int Warnings { get; set; }
    public int Failed { get; set; }
    public IReadOnlyList<SystemReadinessItemDto> Items { get; set; } = Array.Empty<SystemReadinessItemDto>();
}

public sealed class SystemReadinessItemDto
{
    public string Area { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public override string ToString() => Area;
}
