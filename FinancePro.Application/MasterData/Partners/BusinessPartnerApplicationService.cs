using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.MasterData.Partners;

public sealed class BusinessPartnerApplicationService
{
    private readonly IBusinessPartnerGateway _gateway;

    public BusinessPartnerApplicationService(IBusinessPartnerGateway gateway) => _gateway = gateway;

    public async Task<Result<IReadOnlyList<BusinessPartnerDto>>> ListAsync(int empresaId, string? tipo, string? pesquisa, CancellationToken cancellationToken = default)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<BusinessPartnerDto>>.Fail("Empresa inválida.");
        try
        {
            var items = await _gateway.ListAsync(empresaId, NormalizarTipoOpcional(tipo), pesquisa?.Trim(), cancellationToken);
            return Result<IReadOnlyList<BusinessPartnerDto>>.Ok(items);
        }
        catch (Exception ex) { return Result<IReadOnlyList<BusinessPartnerDto>>.Fail(ex.Message); }
    }

    public async Task<Result<BusinessPartnerDto>> GetAsync(int empresaId, string tipo, int id, CancellationToken cancellationToken = default)
    {
        if (empresaId <= 0 || id <= 0) return Result<BusinessPartnerDto>.Fail("Parceiro inválido.");
        var normalizedType = NormalizarTipo(tipo);
        if (normalizedType is null) return Result<BusinessPartnerDto>.Fail("Tipo de parceiro inválido.");
        try
        {
            var item = await _gateway.GetAsync(empresaId, normalizedType, id, cancellationToken);
            return item is null ? Result<BusinessPartnerDto>.Fail("Parceiro não encontrado.") : Result<BusinessPartnerDto>.Ok(item);
        }
        catch (Exception ex) { return Result<BusinessPartnerDto>.Fail(ex.Message); }
    }

    public async Task<Result<int>> SaveAsync(BusinessPartnerSaveRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var tipo = NormalizarTipo(request.Tipo);
        if (request.EmpresaId <= 0) errors.Add("Empresa inválida.");
        if (tipo is null) errors.Add("Selecione Cliente ou Fornecedor.");
        if (string.IsNullOrWhiteSpace(request.Nome) || request.Nome.Trim().Length < 3) errors.Add("O nome deve ter pelo menos 3 caracteres.");
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim();
            if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@')) errors.Add("Indique um email válido.");
        }
        if (errors.Count > 0) return Result<int>.Fail(errors);

        try
        {
            var normalized = request with
            {
                Tipo = tipo!, Nome = request.Nome.Trim(), NIF = Normalizar(request.NIF),
                Telefone = Normalizar(request.Telefone), Email = Normalizar(request.Email)?.ToLowerInvariant(), Morada = Normalizar(request.Morada)
            };
            var id = await _gateway.SaveAsync(normalized, cancellationToken);
            return Result<int>.Ok(id, request.Id == 0 ? $"{tipo} criado com sucesso." : $"{tipo} atualizado com sucesso.");
        }
        catch (Exception ex) { return Result<int>.Fail(ex.Message); }
    }

    public async Task<Result> SetActiveAsync(int empresaId, string tipo, int id, bool ativo, CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizarTipo(tipo);
        if (empresaId <= 0 || id <= 0 || normalizedType is null) return Result.Fail("Parceiro inválido.");
        try
        {
            await _gateway.SetActiveAsync(empresaId, normalizedType, id, ativo, cancellationToken);
            return Result.Ok(ativo ? "Parceiro ativado." : "Parceiro desativado.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    private static string? NormalizarTipo(string? tipo) => tipo?.Trim().ToLowerInvariant() switch
    {
        "cliente" => "Cliente",
        "fornecedor" => "Fornecedor",
        _ => null
    };
    private static string? NormalizarTipoOpcional(string? tipo) => string.IsNullOrWhiteSpace(tipo) ? null : NormalizarTipo(tipo);
    private static string? Normalizar(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
