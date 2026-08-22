namespace FinancePro.Core.DTOs;

public sealed class ContinuityTestDto
{
    public DateTime CheckedAt { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public int Passed { get; set; }
    public int Failed { get; set; }
    public IReadOnlyList<ContinuityTestItemDto> Items { get; set; } = Array.Empty<ContinuityTestItemDto>();
}

public sealed class ContinuityTestItemDto
{
    public string Test { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public override string ToString() => Test;
}
