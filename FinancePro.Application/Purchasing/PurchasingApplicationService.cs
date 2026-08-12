using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Purchasing;

public sealed class PurchasingApplicationService
{
    private readonly IPurchasingGateway _gateway;

    public PurchasingApplicationService(IPurchasingGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<Result<IReadOnlyList<CompraListItemDto>>> ListarAsync(int empresaId)
    {
        if (empresaId <= 0)
            return Result<IReadOnlyList<CompraListItemDto>>.Fail("Empresa inválida.");

        try
        {
            return Result<IReadOnlyList<CompraListItemDto>>.Ok(await _gateway.ListarAsync(empresaId));
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<CompraListItemDto>>.Fail(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyList<FornecedorOpcaoDto>>> ListarFornecedoresAsync(int empresaId)
    {
        if (empresaId <= 0)
            return Result<IReadOnlyList<FornecedorOpcaoDto>>.Fail("Empresa inválida.");

        try
        {
            return Result<IReadOnlyList<FornecedorOpcaoDto>>.Ok(await _gateway.ListarFornecedoresAsync(empresaId));
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<FornecedorOpcaoDto>>.Fail(ex.Message);
        }
    }

    public async Task<Result> CriarAsync(NovaCompraDto dto)
    {
        var erros = Validar(dto);
        if (erros.Count > 0)
            return Result.Fail(erros);

        try
        {
            await _gateway.CriarAsync(dto);
            return Result.Ok("Pedido de compra criado com sucesso.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public Task<Result> CotarAsync(int compraId) => ExecutarAcaoAsync(compraId, _gateway.CotarAsync, "Cotação registada.");

    public Task<Result> AprovarAsync(int compraId) => ExecutarAcaoAsync(
        compraId,
        _gateway.AprovarAsync,
        "Pedido de compra aprovado.");

    public Task<Result> RejeitarAsync(int compraId) => ExecutarAcaoAsync(
        compraId,
        _gateway.RejeitarAsync,
        "Pedido de compra rejeitado.");

    public Task<Result> CancelarAsync(int compraId) => ExecutarAcaoAsync(
        compraId,
        _gateway.CancelarAsync,
        "Pedido de compra cancelado.");

    public Task<Result> EmitirOrdemAsync(int compraId) => ExecutarAcaoAsync(compraId, _gateway.EmitirOrdemAsync, "Ordem de compra emitida.");
    public Task<Result> ReceberAsync(int compraId) => ExecutarAcaoAsync(compraId, _gateway.ReceberAsync, "Receção registada.");
    public Task<Result> FaturarAsync(int compraId) => ExecutarAcaoAsync(compraId, _gateway.FaturarAsync, "Fatura registada e conta a pagar criada.");

    private static IReadOnlyList<string> Validar(NovaCompraDto dto)
    {
        var erros = new List<string>();

        if (dto.EmpresaId <= 0) erros.Add("Empresa inválida.");
        if (dto.Data == default) erros.Add("A data do pedido é obrigatória.");
        if (dto.ValorTotal <= 0) erros.Add("O valor total tem de ser maior do que zero.");
        if (dto.FornecedorId is null or <= 0) erros.Add("Selecione um fornecedor.");
        if (string.IsNullOrWhiteSpace(dto.Departamento)) erros.Add("O departamento é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Comprador)) erros.Add("O comprador é obrigatório.");

        return erros;
    }

    private static async Task<Result> ExecutarAcaoAsync(
        int compraId,
        Func<int, Task> acao,
        string mensagemSucesso)
    {
        if (compraId <= 0)
            return Result.Fail("Pedido de compra inválido.");

        try
        {
            await acao(compraId);
            return Result.Ok(mensagemSucesso);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
