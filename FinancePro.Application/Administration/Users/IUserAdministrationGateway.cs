using FinancePro.Core.DTOs;

namespace FinancePro.Application.Administration.Users;

public interface IUserAdministrationGateway
{
    Task<IReadOnlyList<UtilizadorDto>> ListAsync(string? search, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(UserSaveRequest request, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default);
}
