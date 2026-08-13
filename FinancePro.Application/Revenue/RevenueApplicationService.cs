using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Revenue;

public sealed class RevenueApplicationService
{
    private readonly IRevenueGateway _gateway;

    public RevenueApplicationService(IRevenueGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<Result<IReadOnlyList<ContaReceberListItemDto>>> ListarAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<ContaReceberListItemDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<ContaReceberListItemDto>>.Ok(await _gateway.ListarAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<ContaReceberListItemDto>>.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<ClienteOpcaoDto>>> ListarClientesAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<ClienteOpcaoDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<ClienteOpcaoDto>>.Ok(await _gateway.ListarClientesAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<ClienteOpcaoDto>>.Fail(ex.Message); }
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

    public async Task<Result> CriarAsync(NovaContaReceberDto dto)
    {
        var erros = Validar(dto);
        if (erros.Count > 0) return Result.Fail(erros);

        try
        {
            await _gateway.CriarAsync(dto);
            return Result.Ok("Conta a receber registada com sucesso.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> RegistarRecebimentoAsync(int contaReceberId, string origemTipo, int origemId, DateTime dataRecebimento)
    {
        if (contaReceberId <= 0) return Result.Fail("Conta a receber inválida.");
        if (origemId <= 0) return Result.Fail("Selecione a origem do recebimento.");
        if (origemTipo is not ("Caixa" or "ContaBancaria")) return Result.Fail("Origem de recebimento inválida.");
        if (dataRecebimento == default) return Result.Fail("Indique a data do recebimento.");

        try
        {
            await _gateway.RegistarRecebimentoAsync(contaReceberId, origemTipo, origemId, dataRecebimento);
            return Result.Ok("Recebimento confirmado e lançado na Tesouraria.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> RegistarRecebimentoParcialAsync(int contaReceberId, decimal valor, string origemTipo, int origemId, DateTime dataRecebimento)
    {
        if (contaReceberId <= 0 || valor <= 0) return Result.Fail("Conta e valor de recebimento devem ser válidos.");
        if (origemId <= 0 || origemTipo is not ("Caixa" or "ContaBancaria")) return Result.Fail("Selecione uma origem válida.");
        try { await _gateway.RegistarRecebimentoParcialAsync(contaReceberId, valor, origemTipo, origemId, dataRecebimento); return Result.Ok("Recebimento registado. O saldo da conta foi atualizado."); }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> AprovarPropostaAsync(int contaReceberId)
    {
        if (contaReceberId <= 0) return Result.Fail("Proposta inválida.");
        try { await _gateway.AprovarPropostaAsync(contaReceberId); return Result.Ok("Proposta aprovada."); }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> FaturarAsync(int contaReceberId)
    {
        if (contaReceberId <= 0) return Result.Fail("Proposta inválida.");
        try { await _gateway.FaturarAsync(contaReceberId); return Result.Ok("Fatura emitida e conta a receber ativada."); }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> CancelarAsync(int contaReceberId)
    {
        if (contaReceberId <= 0) return Result.Fail("Conta a receber inválida.");
        try
        {
            await _gateway.CancelarAsync(contaReceberId);
            return Result.Ok("Conta a receber cancelada.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    private static IReadOnlyList<string> Validar(NovaContaReceberDto dto)
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

    public async Task<Result<IReadOnlyList<ProdutoStockDto>>> ListarProdutosAsync(int empresaId){ try{return Result<IReadOnlyList<ProdutoStockDto>>.Ok(await _gateway.ListarProdutosAsync(empresaId));}catch(Exception ex){return Result<IReadOnlyList<ProdutoStockDto>>.Fail(ex.Message);} }
    public async Task<Result<IReadOnlyList<DocumentoItemDto>>> ListarItensAsync(int documentoId){ try{return Result<IReadOnlyList<DocumentoItemDto>>.Ok(await _gateway.ListarItensAsync(documentoId));}catch(Exception ex){return Result<IReadOnlyList<DocumentoItemDto>>.Fail(ex.Message);} }
    public async Task<Result> AdicionarItemAsync(NovoDocumentoItemDto dto){ try{await _gateway.AdicionarItemAsync(dto); return Result.Ok("Item adicionado e total recalculado.");}catch(Exception ex){return Result.Fail(ex.Message);} }
    public async Task<Result> RemoverItemAsync(int itemId){ try{await _gateway.RemoverItemAsync(itemId); return Result.Ok("Item removido e total recalculado.");}catch(Exception ex){return Result.Fail(ex.Message);} }
    public async Task<Result<IReadOnlyList<DocumentoFiscalDto>>> ListarDocumentosAsync(int contaReceberId)
    {
        if (contaReceberId <= 0) return Result<IReadOnlyList<DocumentoFiscalDto>>.Fail("Documento inválido.");
        try { return Result<IReadOnlyList<DocumentoFiscalDto>>.Ok(await _gateway.ListarDocumentosAsync(contaReceberId)); }
        catch (Exception ex) { return Result<IReadOnlyList<DocumentoFiscalDto>>.Fail(ex.Message); }
    }

    public async Task<Result> EmitirNotaAsync(NovaNotaFiscalDto dto)
    {
        if (dto.ContaReceberId <= 0 || dto.Valor <= 0) return Result.Fail("Documento e valor devem ser válidos.");
        if (dto.Tipo is not ("Credito" or "Debito")) return Result.Fail("Tipo de nota inválido.");
        if (string.IsNullOrWhiteSpace(dto.Motivo)) return Result.Fail("Indique o motivo da nota.");
        try { await _gateway.EmitirNotaAsync(dto); return Result.Ok($"Nota de {dto.Tipo.ToLowerInvariant()} emitida."); }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

}
