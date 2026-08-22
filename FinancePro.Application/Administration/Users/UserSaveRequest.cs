namespace FinancePro.Application.Administration.Users;

public sealed record UserSaveRequest(
    int Id,
    string FullName,
    string Email,
    int ProfileId,
    int CompanyId,
    bool IsActive,
    string? NewPassword,
    byte[]? ProfilePhoto = null);
