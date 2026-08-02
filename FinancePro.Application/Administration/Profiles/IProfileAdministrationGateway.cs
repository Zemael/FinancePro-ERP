using FinancePro.Core.DTOs;

namespace FinancePro.Application.Administration.Profiles;

public interface IProfileAdministrationGateway
{
    Task<IReadOnlyList<PerfilDto>> ListAsync(string? search, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(ProfileSaveRequest request, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default);
}
