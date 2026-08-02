using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Administration.Permissions;

public sealed class PermissionAdministrationService
{
    private readonly IPermissionAdministrationGateway _gateway;

    public PermissionAdministrationService(IPermissionAdministrationGateway gateway) => _gateway = gateway;

    public async Task<Result<IReadOnlyList<PermissaoPerfilDto>>> GetMatrixAsync(int profileId, CancellationToken cancellationToken = default)
    {
        if (profileId <= 0) return Result<IReadOnlyList<PermissaoPerfilDto>>.Fail("Selecione um perfil.");
        try
        {
            return Result<IReadOnlyList<PermissaoPerfilDto>>.Ok(await _gateway.GetMatrixAsync(profileId, cancellationToken));
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<PermissaoPerfilDto>>.Fail(ex.Message);
        }
    }

    public async Task<Result> SaveMatrixAsync(int profileId, IReadOnlyCollection<PermissaoPerfilDto>? permissions, CancellationToken cancellationToken = default)
    {
        if (profileId <= 0) return Result.Fail("Selecione um perfil.");
        if (permissions is null || permissions.Count == 0) return Result.Fail("A matriz de permissões está vazia.");
        if (permissions.Any(x => x.PerfilId != profileId)) return Result.Fail("A matriz contém permissões de outro perfil.");

        try
        {
            await _gateway.SaveMatrixAsync(profileId, permissions, cancellationToken);
            return Result.Ok("Permissões guardadas com sucesso.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
