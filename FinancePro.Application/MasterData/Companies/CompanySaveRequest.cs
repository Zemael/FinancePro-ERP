namespace FinancePro.Application.MasterData.Companies;

public sealed record CompanySaveRequest(
    int Id,
    string Name,
    string? TaxNumber,
    string? Address,
    string? Phone,
    string? Email,
    string Currency);
