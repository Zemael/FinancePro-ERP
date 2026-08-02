using FinancePro.Core.DTOs;

namespace FinancePro.Application.Administration.Permissions;

public interface IPermissionAdministrationGateway
{
    Task<IReadOnlyList<PermissaoPerfilDto>> GetMatrixAsync(int profileId, CancellationToken cancellationToken = default);
    Task SaveMatrixAsync(int profileId, IReadOnlyCollection<PermissaoPerfilDto> permissions, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetKeysAsync(int profileId, CancellationToken cancellationToken = default);
}
