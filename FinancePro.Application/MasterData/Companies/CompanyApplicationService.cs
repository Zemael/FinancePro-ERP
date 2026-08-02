using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.MasterData.Companies;

public sealed class CompanyApplicationService
{
    private readonly ICompanyGateway _gateway;

    public CompanyApplicationService(ICompanyGateway gateway) => _gateway = gateway;

    public async Task<Result<IReadOnlyList<EmpresaListItemDto>>> ListAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var companies = await _gateway.ListAsync(search?.Trim(), cancellationToken);
            return Result<IReadOnlyList<EmpresaListItemDto>>.Ok(companies);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<EmpresaListItemDto>>.Fail(ex.Message);
        }
    }

    public async Task<Result<EmpresaDto>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return Result<EmpresaDto>.Fail("Empresa inválida.");

        try
        {
            var company = await _gateway.GetAsync(id, cancellationToken);
            return company is null
                ? Result<EmpresaDto>.Fail("Empresa não encontrada.")
                : Result<EmpresaDto>.Ok(company);
        }
        catch (Exception ex)
        {
            return Result<EmpresaDto>.Fail(ex.Message);
        }
    }

    public async Task<Result<int>> SaveAsync(
        CompanySaveRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = Validate(request);
        if (validation.IsFailure)
            return Result<int>.Fail(validation.Errors, validation.Message);

        try
        {
            var normalized = request with
            {
                Name = request.Name.Trim(),
                TaxNumber = NormalizeOptional(request.TaxNumber),
                Address = NormalizeOptional(request.Address),
                Phone = NormalizeOptional(request.Phone),
                Email = NormalizeOptional(request.Email)?.ToLowerInvariant(),
                Currency = request.Currency.Trim().ToUpperInvariant()
            };

            var id = await _gateway.SaveAsync(normalized, cancellationToken);
            return Result<int>.Ok(id, request.Id == 0
                ? "Empresa criada com sucesso."
                : "Empresa atualizada com sucesso.");
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex.Message);
        }
    }

    public async Task<Result> SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return Result.Fail("Empresa inválida.");

        try
        {
            await _gateway.SetActiveAsync(id, active, cancellationToken);
            return Result.Ok(active ? "Empresa ativada." : "Empresa desativada.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    private static Result Validate(CompanySaveRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length < 3)
            errors.Add("O nome da empresa deve ter pelo menos 3 caracteres.");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim();
            if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
                errors.Add("Indique um email válido.");
        }

        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Trim().Length is < 3 or > 5)
            errors.Add("Indique uma moeda válida.");

        return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
