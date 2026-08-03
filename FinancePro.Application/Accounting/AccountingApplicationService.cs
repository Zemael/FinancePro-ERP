using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Accounting;

public sealed class AccountingApplicationService
{
    private readonly IAccountingGateway _gateway;
    public AccountingApplicationService(IAccountingGateway gateway) => _gateway = gateway;

    public async Task<Result<IReadOnlyList<PlanoContaDto>>> ListAccountsAsync(int empresaId, string? pesquisa = null, CancellationToken ct = default)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<PlanoContaDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<PlanoContaDto>>.Ok(await _gateway.ListAccountsAsync(empresaId, pesquisa?.Trim(), ct)); }
        catch (Exception ex) { return Result<IReadOnlyList<PlanoContaDto>>.Fail(ex.Message, "Não foi possível carregar o plano de contas."); }
    }

    public async Task<Result<int>> SaveAccountAsync(AccountSaveRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();
        if (request.EmpresaId <= 0) errors.Add("Empresa inválida.");
        if (string.IsNullOrWhiteSpace(request.Codigo)) errors.Add("Informe o código da conta.");
        if (string.IsNullOrWhiteSpace(request.Nome)) errors.Add("Informe o nome da conta.");
        if (errors.Count > 0) return Result<int>.Fail(errors);
        var codigo = request.Codigo.Trim();
        if (await _gateway.AccountCodeExistsAsync(request.EmpresaId, codigo, request.Id, ct)) return Result<int>.Fail("Já existe uma conta com este código.");
        try { return Result<int>.Ok(await _gateway.SaveAccountAsync(request with { Codigo = codigo, Nome = request.Nome.Trim() }, ct), request.Id == 0 ? "Conta criada." : "Conta atualizada."); }
        catch (Exception ex) { return Result<int>.Fail(ex.Message, "Não foi possível guardar a conta."); }
    }

    public async Task<Result<int>> SaveEntryAsync(JournalEntryRequest request, CancellationToken ct = default)
    {
        var errors = ValidateEntry(request);
        if (errors.Count > 0) return Result<int>.Fail(errors);
        try { return Result<int>.Ok(await _gateway.SaveEntryAsync(request with { Descricao = request.Descricao.Trim() }, ct), "Lançamento guardado em rascunho."); }
        catch (Exception ex) { return Result<int>.Fail(ex.Message, "Não foi possível guardar o lançamento."); }
    }

    public async Task<Result> PostEntryAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return Result.Fail("Lançamento inválido.");
        try { await _gateway.PostEntryAsync(id, ct); return Result.Ok("Lançamento contabilizado com sucesso."); }
        catch (Exception ex) { return Result.Fail(ex.Message, "Não foi possível contabilizar o lançamento."); }
    }

    public Task<Result<IReadOnlyList<LancamentoContabilDto>>> ListEntriesAsync(int empresaId, CancellationToken ct = default) => ListEntriesCoreAsync(empresaId, ct);
    private async Task<Result<IReadOnlyList<LancamentoContabilDto>>> ListEntriesCoreAsync(int empresaId, CancellationToken ct)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<LancamentoContabilDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<LancamentoContabilDto>>.Ok(await _gateway.ListEntriesAsync(empresaId, ct)); }
        catch (Exception ex) { return Result<IReadOnlyList<LancamentoContabilDto>>.Fail(ex.Message, "Não foi possível carregar os lançamentos."); }
    }

    private static List<string> ValidateEntry(JournalEntryRequest request)
    {
        var errors = new List<string>();
        if (request.EmpresaId <= 0) errors.Add("Empresa inválida.");
        if (string.IsNullOrWhiteSpace(request.Descricao)) errors.Add("Informe a descrição do lançamento.");
        if (request.Linhas is null || request.Linhas.Count < 2) errors.Add("Informe pelo menos duas linhas.");
        else
        {
            foreach (var line in request.Linhas)
            {
                if (line.PlanoContasId <= 0) errors.Add("Todas as linhas devem ter uma conta.");
                if (line.Debito < 0 || line.Credito < 0) errors.Add("Débito e crédito não podem ser negativos.");
                if ((line.Debito > 0) == (line.Credito > 0)) errors.Add("Cada linha deve conter débito ou crédito, nunca ambos.");
            }
            var debito = request.Linhas.Sum(x => x.Debito);
            var credito = request.Linhas.Sum(x => x.Credito);
            if (debito <= 0 || debito != credito) errors.Add("O total de débitos deve ser igual ao total de créditos.");
        }
        return errors.Distinct().ToList();
    }
}
