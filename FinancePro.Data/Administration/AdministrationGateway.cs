using FinancePro.Application.Administration.Permissions;
using FinancePro.Application.Administration.Profiles;
using FinancePro.Application.Administration.Users;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.Administration;

public sealed class AdministrationGateway :
    IUserAdministrationGateway,
    IProfileAdministrationGateway,
    IPermissionAdministrationGateway
{
    private readonly IUtilizadorService _users;
    private readonly IPerfilService _profiles;
    private readonly IPermissaoService _permissions;

    public AdministrationGateway(
        IUtilizadorService users,
        IPerfilService profiles,
        IPermissaoService permissions)
    {
        _users = users;
        _profiles = profiles;
        _permissions = permissions;
    }

    public Task<IReadOnlyList<UtilizadorDto>> ListAsync(string? search, CancellationToken cancellationToken = default)
        => _users.ListarAsync(search);

    public Task<int> SaveAsync(UserSaveRequest request, CancellationToken cancellationToken = default)
        => _users.GuardarAsync(new UtilizadorDto
        {
            Id = request.Id,
            NomeCompleto = request.FullName,
            Email = request.Email,
            PerfilId = request.ProfileId,
            EmpresaId = request.CompanyId,
            Ativo = request.IsActive,
            NovaPassword = request.NewPassword,
            FotoPerfil = request.ProfilePhoto
        });

    public Task SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default)
        => _users.AlternarAtivoAsync(id, active);

    Task<IReadOnlyList<PerfilDto>> IProfileAdministrationGateway.ListAsync(string? search, CancellationToken cancellationToken)
        => _profiles.ListarAsync(search);

    Task<int> IProfileAdministrationGateway.SaveAsync(ProfileSaveRequest request, CancellationToken cancellationToken)
        => _profiles.GuardarAsync(new PerfilDto
        {
            Id = request.Id,
            Nome = request.Name,
            Descricao = request.Description,
            Ativo = request.IsActive
        });

    Task IProfileAdministrationGateway.SetActiveAsync(int id, bool active, CancellationToken cancellationToken)
        => _profiles.AlternarAtivoAsync(id, active);

    public Task<IReadOnlyList<PermissaoPerfilDto>> GetMatrixAsync(int profileId, CancellationToken cancellationToken = default)
        => _permissions.ObterMatrizAsync(profileId);

    public Task SaveMatrixAsync(int profileId, IReadOnlyCollection<PermissaoPerfilDto> permissions, CancellationToken cancellationToken = default)
        => _permissions.GuardarMatrizAsync(profileId, permissions);

    public Task<IReadOnlyCollection<string>> GetKeysAsync(int profileId, CancellationToken cancellationToken = default)
        => _permissions.ObterChavesAsync(profileId);
}
