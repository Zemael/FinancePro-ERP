namespace FinancePro.Application.Administration.Profiles;

public sealed record ProfileSaveRequest(int Id, string Name, string? Description, bool IsActive);
