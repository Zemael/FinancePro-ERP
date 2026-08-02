using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.MasterData.Banking;

public sealed class BankingMasterDataService
{
    private readonly IBankingMasterDataGateway _gateway;

    public BankingMasterDataService(IBankingMasterDataGateway gateway) => _gateway = gateway;

    public async Task<Result<IReadOnlyList<BancoListItemDto>>> ListBanksAsync(CancellationToken cancellationToken = default)
    {
        try { return Result<IReadOnlyList<BancoListItemDto>>.Ok(await _gateway.ListBanksAsync(cancellationToken)); }
        catch (Exception ex) { return Result<IReadOnlyList<BancoListItemDto>>.Fail(ex.Message); }
    }

    public async Task<Result> CreateBankAsync(string? name, string? swift, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 3) errors.Add("O nome do banco deve ter pelo menos 3 caracteres.");
        var normalizedSwift = string.IsNullOrWhiteSpace(swift) ? null : swift.Trim().ToUpperInvariant();
        if (normalizedSwift is { Length: > 0 } && normalizedSwift.Length is < 8 or > 11) errors.Add("O código SWIFT deve ter entre 8 e 11 caracteres.");
        if (errors.Count > 0) return Result.Fail(errors);

        try
        {
            await _gateway.CreateBankAsync(new NovoBancoDto { Nome = name!.Trim(), CodigoSwift = normalizedSwift }, cancellationToken);
            return Result.Ok("Banco criado com sucesso.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<ContaBancariaListItemDto>>> ListAccountsAsync(int companyId, CancellationToken cancellationToken = default)
    {
        if (companyId <= 0) return Result<IReadOnlyList<ContaBancariaListItemDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<ContaBancariaListItemDto>>.Ok(await _gateway.ListAccountsAsync(companyId, cancellationToken)); }
        catch (Exception ex) { return Result<IReadOnlyList<ContaBancariaListItemDto>>.Fail(ex.Message); }
    }

    public async Task<Result> CreateAccountAsync(NovaContaBancariaDto request, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        if (request.EmpresaId <= 0) errors.Add("Empresa inválida.");
        if (request.BancoId <= 0) errors.Add("Selecione o banco.");
        if (string.IsNullOrWhiteSpace(request.NumeroConta)) errors.Add("Indique o número da conta.");
        if (string.IsNullOrWhiteSpace(request.Titular)) errors.Add("Indique o titular da conta.");
        if (request.SaldoInicial < 0) errors.Add("O saldo inicial não pode ser negativo.");
        if (errors.Count > 0) return Result.Fail(errors);

        request.NumeroConta = request.NumeroConta.Trim();
        request.IBAN = string.IsNullOrWhiteSpace(request.IBAN) ? null : request.IBAN.Trim().Replace(" ", string.Empty).ToUpperInvariant();
        request.Titular = request.Titular.Trim();
        request.Moeda = string.IsNullOrWhiteSpace(request.Moeda) ? "FCFA" : request.Moeda.Trim().ToUpperInvariant();

        try
        {
            await _gateway.CreateAccountAsync(request, cancellationToken);
            return Result.Ok("Conta bancária criada com sucesso.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> SetAccountActiveAsync(int accountId, bool active, CancellationToken cancellationToken = default)
    {
        if (accountId <= 0) return Result.Fail("Conta bancária inválida.");
        try
        {
            await _gateway.SetAccountActiveAsync(accountId, active, cancellationToken);
            return Result.Ok(active ? "Conta bancária ativada." : "Conta bancária desativada.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }
}
