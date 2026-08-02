using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.MasterData.Currencies;

public sealed class CurrencyMasterDataService
{
    private readonly ICurrencyMasterDataGateway _gateway;
    public CurrencyMasterDataService(ICurrencyMasterDataGateway gateway) => _gateway = gateway;

    public async Task<Result<IReadOnlyList<MoedaDto>>> ListAsync(string? search, CancellationToken cancellationToken = default)
    {
        try { return Result<IReadOnlyList<MoedaDto>>.Ok(await _gateway.ListAsync(search?.Trim(), cancellationToken)); }
        catch (Exception ex) { return Result<IReadOnlyList<MoedaDto>>.Fail(ex.Message); }
    }

    public async Task<Result<int>> SaveAsync(MoedaDto currency, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(currency.CodigoIso) || currency.CodigoIso.Trim().Length != 3) errors.Add("O código ISO deve conter 3 caracteres.");
        if (string.IsNullOrWhiteSpace(currency.Nome) || currency.Nome.Trim().Length < 3) errors.Add("Indique o nome da moeda.");
        if (currency.CasasDecimais is < 0 or > 4) errors.Add("As casas decimais devem estar entre 0 e 4.");
        if (errors.Count > 0) return Result<int>.Fail(errors);

        currency.CodigoIso = currency.CodigoIso.Trim().ToUpperInvariant();
        currency.Nome = currency.Nome.Trim();
        currency.Simbolo = currency.Simbolo?.Trim() ?? string.Empty;

        try
        {
            var id = await _gateway.SaveAsync(currency, cancellationToken);
            return Result<int>.Ok(id, currency.Id == 0 ? "Moeda criada com sucesso." : "Moeda atualizada com sucesso.");
        }
        catch (Exception ex) { return Result<int>.Fail(ex.Message); }
    }

    public async Task<Result> SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default)
    {
        if (id <= 0) return Result.Fail("Moeda inválida.");
        try
        {
            await _gateway.SetActiveAsync(id, active, cancellationToken);
            return Result.Ok(active ? "Moeda ativada." : "Moeda desativada.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }
}
