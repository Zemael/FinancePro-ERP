namespace FinancePro.Application.Common.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    int UserId { get; }
    int CompanyId { get; }
    string FullName { get; }
    string ProfileName { get; }
    bool HasPermission(string module, string action = "Consultar");
}
