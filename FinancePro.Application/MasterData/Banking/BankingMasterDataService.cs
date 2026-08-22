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

    public async Task<Result> CreateBankAsync(string? name, string? abbreviation, string? swift, string? address, string? contact, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 3) errors.Add("O nome do banco deve ter pelo menos 3 caracteres.");
        var normalizedAbbreviation = string.IsNullOrWhiteSpace(abbreviation) ? null : abbreviation.Trim().ToUpperInvariant();
        if (normalizedAbbreviation is { Length: > 20 }) errors.Add("A sigla do banco não pode ultrapassar 20 caracteres.");
        var normalizedSwift = string.IsNullOrWhiteSpace(swift) ? null : swift.Trim().ToUpperInvariant();
        if (normalizedSwift is { Length: > 0 } && normalizedSwift.Length is < 8 or > 11) errors.Add("O código SWIFT deve ter entre 8 e 11 caracteres.");
        var normalizedAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        if (normalizedAddress is { Length: > 250 }) errors.Add("O endereço do banco não pode ultrapassar 250 caracteres.");
        var normalizedContact = string.IsNullOrWhiteSpace(contact) ? null : contact.Trim();
        if (normalizedContact is { Length: > 50 }) errors.Add("O contacto do banco não pode ultrapassar 50 caracteres.");
        if (errors.Count > 0) return Result.Fail(errors);

        try
        {
            await _gateway.CreateBankAsync(new NovoBancoDto
            {
                Nome = name!.Trim(),
                Sigla = normalizedAbbreviation,
                CodigoSwift = normalizedSwift,
                Endereco = normalizedAddress,
                Contacto = normalizedContact
            }, cancellationToken);
            return Result.Ok("Banco criado com sucesso.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> UpdateBankAsync(int bankId, string? name, string? abbreviation, string? swift, string? address, string? contact, CancellationToken cancellationToken = default)
    {
        if (bankId <= 0) return Result.Fail("Banco inválido.");

        var request = NormalizeBank(name, abbreviation, swift, address, contact, out var errors);
        if (errors.Count > 0) return Result.Fail(errors);

        try
        {
            await _gateway.UpdateBankAsync(bankId, request, cancellationToken);
            return Result.Ok("Banco atualizado com sucesso.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    private static NovoBancoDto NormalizeBank(string? name, string? abbreviation, string? swift, string? address, string? contact, out List<string> errors)
    {
        errors = new List<string>();
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 3) errors.Add("O nome do banco deve ter pelo menos 3 caracteres.");
        var normalizedAbbreviation = string.IsNullOrWhiteSpace(abbreviation) ? null : abbreviation.Trim().ToUpperInvariant();
        if (normalizedAbbreviation is { Length: > 20 }) errors.Add("A sigla do banco não pode ultrapassar 20 caracteres.");
        var normalizedSwift = string.IsNullOrWhiteSpace(swift) ? null : swift.Trim().ToUpperInvariant();
        if (normalizedSwift is { Length: > 0 } && normalizedSwift.Length is < 8 or > 11) errors.Add("O código SWIFT deve ter entre 8 e 11 caracteres.");
        var normalizedAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        if (normalizedAddress is { Length: > 250 }) errors.Add("O endereço do banco não pode ultrapassar 250 caracteres.");
        var normalizedContact = string.IsNullOrWhiteSpace(contact) ? null : contact.Trim();
        if (normalizedContact is { Length: > 50 }) errors.Add("O contacto do banco não pode ultrapassar 50 caracteres.");
        return new NovoBancoDto { Nome = name?.Trim() ?? string.Empty, Sigla = normalizedAbbreviation, CodigoSwift = normalizedSwift, Endereco = normalizedAddress, Contacto = normalizedContact };
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

    public async Task<Result> UpdateAccountAsync(int accountId, NovaContaBancariaDto request, CancellationToken cancellationToken = default)
    {
        if (accountId <= 0) return Result.Fail("Conta bancária inválida.");
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
        try { await _gateway.UpdateAccountAsync(accountId, request, cancellationToken); return Result.Ok("Conta bancária atualizada."); }
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
