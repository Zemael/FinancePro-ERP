using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;

namespace FinancePro.Application.Budget;

public sealed class BudgetApplicationService
{
    private readonly IBudgetGateway _gateway;
    public BudgetApplicationService(IBudgetGateway gateway) => _gateway = gateway;

    public async Task<Result<IReadOnlyList<OrcamentoListItemDto>>> ListarAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<OrcamentoListItemDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<OrcamentoListItemDto>>.Ok(await _gateway.ListarAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<OrcamentoListItemDto>>.Fail(ex.Message); }
    }

    public async Task<Result<int>> CriarAsync(NovoOrcamentoDto dto)
    {
        var erros = new List<string>();
        if (dto.EmpresaId <= 0) erros.Add("Empresa inválida.");
        if (string.IsNullOrWhiteSpace(dto.Nome)) erros.Add("O nome do orçamento é obrigatório.");
        if (dto.Ano is < 2000 or > 2100) erros.Add("Indique um ano válido.");
        if (dto.DataInicio == default || dto.DataFim == default) erros.Add("Indique o período do orçamento.");
        if (dto.DataFim < dto.DataInicio) erros.Add("A data de fim não pode ser anterior à data de início.");
        if (erros.Count > 0) return Result<int>.Fail(erros);
        try { return Result<int>.Ok(await _gateway.CriarAsync(dto), "Orçamento criado com sucesso."); }
        catch (Exception ex) { return Result<int>.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<PlanoContasOpcaoDto>>> ListarPlanoContasAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<PlanoContasOpcaoDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<PlanoContasOpcaoDto>>.Ok(await _gateway.ListarPlanoContasAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<PlanoContasOpcaoDto>>.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<OrcamentoDetalheDto>>> ListarDetalhesAsync(int id, TipoCategoria tipo)
    {
        if (id <= 0) return Result<IReadOnlyList<OrcamentoDetalheDto>>.Fail("Orçamento inválido.");
        try { return Result<IReadOnlyList<OrcamentoDetalheDto>>.Ok(await _gateway.ListarDetalhesAsync(id, tipo)); }
        catch (Exception ex) { return Result<IReadOnlyList<OrcamentoDetalheDto>>.Fail(ex.Message); }
    }

    public async Task<Result> AdicionarDetalheAsync(NovoOrcamentoDetalheDto dto)
    {
        var erros = new List<string>();
        if (dto.OrcamentoId <= 0) erros.Add("Orçamento inválido.");
        if (dto.PlanoContasId <= 0) erros.Add("Selecione a conta do plano de contas.");
        if (dto.Mes is < 1 or > 12) erros.Add("Mês inválido.");
        if (dto.ValorPrevisto < 0) erros.Add("O valor previsto não pode ser negativo.");
        if (erros.Count > 0) return Result.Fail(erros);
        try { await _gateway.AdicionarDetalheAsync(dto); return Result.Ok("Linha orçamental adicionada."); }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<ExecucaoMensalDto>>> ObterExecucaoMensalAsync(int id)
    {
        if (id <= 0) return Result<IReadOnlyList<ExecucaoMensalDto>>.Fail("Orçamento inválido.");
        try { return Result<IReadOnlyList<ExecucaoMensalDto>>.Ok(await _gateway.ObterExecucaoMensalAsync(id)); }
        catch (Exception ex) { return Result<IReadOnlyList<ExecucaoMensalDto>>.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<RevisaoOrcamentalDto>>> ListarRevisoesAsync(int id)
    {
        if (id <= 0) return Result<IReadOnlyList<RevisaoOrcamentalDto>>.Fail("Orçamento inválido.");
        try { return Result<IReadOnlyList<RevisaoOrcamentalDto>>.Ok(await _gateway.ListarRevisoesAsync(id)); }
        catch (Exception ex) { return Result<IReadOnlyList<RevisaoOrcamentalDto>>.Fail(ex.Message); }
    }

    public async Task<Result> AdicionarRevisaoAsync(NovaRevisaoOrcamentalDto dto)
    {
        var erros = new List<string>();
        if (dto.OrcamentoId <= 0) erros.Add("Orçamento inválido.");
        if (string.IsNullOrWhiteSpace(dto.Motivo)) erros.Add("Indique o motivo da revisão.");
        if (string.IsNullOrWhiteSpace(dto.Responsavel)) erros.Add("Indique o responsável pela revisão.");
        if (erros.Count > 0) return Result.Fail(erros);
        try { await _gateway.AdicionarRevisaoAsync(dto); return Result.Ok("Revisão orçamental adicionada."); }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result<OrcamentoRelatorioDto>> ObterRelatorioAsync(int id)
    {
        if (id <= 0) return Result<OrcamentoRelatorioDto>.Fail("Orçamento inválido.");
        try { return Result<OrcamentoRelatorioDto>.Ok(await _gateway.ObterRelatorioAsync(id)); }
        catch (Exception ex) { return Result<OrcamentoRelatorioDto>.Fail(ex.Message); }
    }
}
