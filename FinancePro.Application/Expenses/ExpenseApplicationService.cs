using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Expenses;

public sealed class ExpenseApplicationService
{
    private readonly IExpenseGateway _gateway;

    public ExpenseApplicationService(IExpenseGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<Result<IReadOnlyList<ContaPagarListItemDto>>> ListarAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<ContaPagarListItemDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<ContaPagarListItemDto>>.Ok(await _gateway.ListarAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<ContaPagarListItemDto>>.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<FornecedorOpcaoDto>>> ListarFornecedoresAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<FornecedorOpcaoDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<FornecedorOpcaoDto>>.Ok(await _gateway.ListarFornecedoresAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<FornecedorOpcaoDto>>.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<CategoriaOpcaoDto>>> ListarCategoriasAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<CategoriaOpcaoDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<CategoriaOpcaoDto>>.Ok(await _gateway.ListarCategoriasAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<CategoriaOpcaoDto>>.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<OpcaoOrigemDto>>> ListarOrigensAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<OpcaoOrigemDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<OpcaoOrigemDto>>.Ok(await _gateway.ListarOrigensAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<OpcaoOrigemDto>>.Fail(ex.Message); }
    }

    public async Task<Result> CriarAsync(NovaContaPagarDto dto)
    {
        var erros = Validar(dto);
        if (erros.Count > 0) return Result.Fail(erros);

        try
        {
            await _gateway.CriarAsync(dto);
            return Result.Ok("Conta a pagar registada com sucesso.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> RegistarPagamentoAsync(int contaPagarId, string origemTipo, int origemId, DateTime dataPagamento)
    {
        if (contaPagarId <= 0) return Result.Fail("Conta a pagar inválida.");
        if (origemId <= 0) return Result.Fail("Selecione a origem do pagamento.");
        if (origemTipo is not ("Caixa" or "ContaBancaria")) return Result.Fail("Origem de pagamento inválida.");
        if (dataPagamento == default) return Result.Fail("Indique a data do pagamento.");

        try
        {
            await _gateway.RegistarPagamentoAsync(contaPagarId, origemTipo, origemId, dataPagamento);
            return Result.Ok("Pagamento confirmado e lançado na Tesouraria.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> CancelarAsync(int contaPagarId)
    {
        if (contaPagarId <= 0) return Result.Fail("Conta a pagar inválida.");
        try
        {
            await _gateway.CancelarAsync(contaPagarId);
            return Result.Ok("Conta a pagar cancelada.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    private static IReadOnlyList<string> Validar(NovaContaPagarDto dto)
    {
        var erros = new List<string>();
        if (dto.EmpresaId <= 0) erros.Add("Empresa inválida.");
        if (string.IsNullOrWhiteSpace(dto.Descricao)) erros.Add("A descrição é obrigatória.");
        if (dto.Valor <= 0) erros.Add("O valor tem de ser maior do que zero.");
        if (dto.DataEmissao == default) erros.Add("A data de emissão é obrigatória.");
        if (dto.DataVencimento == default) erros.Add("A data de vencimento é obrigatória.");
        if (dto.DataVencimento.Date < dto.DataEmissao.Date) erros.Add("A data de vencimento não pode ser anterior à data de emissão.");
        return erros;
    }
}
