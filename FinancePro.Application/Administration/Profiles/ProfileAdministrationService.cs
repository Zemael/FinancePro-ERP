using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Administration.Profiles;

public sealed class ProfileAdministrationService
{
    private readonly IProfileAdministrationGateway _gateway;

    public ProfileAdministrationService(IProfileAdministrationGateway gateway) => _gateway = gateway;

    public async Task<Result<IReadOnlyList<PerfilDto>>> ListAsync(string? search, CancellationToken cancellationToken = default)
    {
        try
        {
            return Result<IReadOnlyList<PerfilDto>>.Ok(await _gateway.ListAsync(search?.Trim(), cancellationToken));
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<PerfilDto>>.Fail(ex.Message);
        }
    }

    public async Task<Result<int>> SaveAsync(ProfileSaveRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length < 3)
            return Result<int>.Fail("O nome do perfil deve ter pelo menos 3 caracteres.");

        try
        {
            var id = await _gateway.SaveAsync(request with
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
            }, cancellationToken);
            return Result<int>.Ok(id, request.Id == 0 ? "Perfil criado com sucesso." : "Perfil atualizado com sucesso.");
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex.Message);
        }
    }

    public async Task<Result> SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default)
    {
        if (id <= 0) return Result.Fail("Perfil inválido.");
        try
        {
            await _gateway.SetActiveAsync(id, active, cancellationToken);
            return Result.Ok(active ? "Perfil ativado." : "Perfil desativado.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
