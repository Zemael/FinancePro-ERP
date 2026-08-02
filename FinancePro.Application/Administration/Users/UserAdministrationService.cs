using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Administration.Users;

public sealed class UserAdministrationService
{
    private readonly IUserAdministrationGateway _gateway;

    public UserAdministrationService(IUserAdministrationGateway gateway) => _gateway = gateway;

    public async Task<Result<IReadOnlyList<UtilizadorDto>>> ListAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _gateway.ListAsync(search?.Trim(), cancellationToken);
            return Result<IReadOnlyList<UtilizadorDto>>.Ok(users);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<UtilizadorDto>>.Fail(ex.Message);
        }
    }

    public async Task<Result<int>> SaveAsync(
        UserSaveRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = Validate(request);
        if (!validation.IsSuccess)
            return Result<int>.Fail(validation.Errors.FirstOrDefault() ?? validation.Message ?? "Os dados do utilizador são inválidos.");

        try
        {
            var id = await _gateway.SaveAsync(request with
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim().ToLowerInvariant()
            }, cancellationToken);

            return Result<int>.Ok(id, request.Id == 0
                ? "Utilizador criado com sucesso."
                : "Utilizador atualizado com sucesso.");
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex.Message);
        }
    }

    public async Task<Result> SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return Result.Fail("Utilizador inválido.");

        try
        {
            await _gateway.SetActiveAsync(id, active, cancellationToken);
            return Result.Ok(active ? "Utilizador ativado." : "Utilizador desativado.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    private static Result Validate(UserSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length < 3)
            return Result.Fail("O nome completo deve ter pelo menos 3 caracteres.");

        var email = request.Email?.Trim() ?? string.Empty;
        if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
            return Result.Fail("Indique um email válido.");

        if (request.ProfileId <= 0)
            return Result.Fail("Selecione um perfil.");

        if (request.CompanyId <= 0)
            return Result.Fail("Selecione uma empresa.");

        if (request.Id == 0 && (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6))
            return Result.Fail("A palavra-passe inicial deve ter pelo menos 6 caracteres.");

        if (!string.IsNullOrEmpty(request.NewPassword) && request.NewPassword.Length < 6)
            return Result.Fail("A nova palavra-passe deve ter pelo menos 6 caracteres.");

        return Result.Ok();
    }
}
