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

    public async Task<Result<EmpresaDto>> ObterEmpresaAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<EmpresaDto>.Fail("Empresa inválida.");
        try
        {
            var empresa = await _gateway.ObterEmpresaAsync(empresaId);
            return empresa is null ? Result<EmpresaDto>.Fail("Empresa não encontrada.") : Result<EmpresaDto>.Ok(empresa);
        }
        catch (Exception ex) { return Result<EmpresaDto>.Fail(ex.Message); }
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
            return Result.Ok(dto.CriarComoProposta ? "Rascunho da fatura proforma criado. Adicione agora os itens." : "Conta a receber registada com sucesso.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> AtualizarRascunhoAsync(AtualizarFaturaRascunhoDto dto)
    {
        var erros = new List<string>();
        if (dto.Id <= 0 || dto.EmpresaId <= 0) erros.Add("Rascunho inválido.");
        if (string.IsNullOrWhiteSpace(dto.Descricao)) erros.Add("A descrição é obrigatória.");
        if (dto.DataVencimento.Date < dto.DataEmissao.Date) erros.Add("A data de vencimento não pode ser anterior à data de emissão.");
        if (dto.DescontoGeral < 0 || dto.Frete < 0 || dto.OutrasDespesas < 0) erros.Add("Desconto, frete e outras despesas não podem ser negativos.");
        if (erros.Count > 0) return Result.Fail(erros);
        try { await _gateway.AtualizarRascunhoAsync(dto); return Result.Ok("Rascunho atualizado com sucesso."); }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result<int>> DuplicarAsync(int contaReceberId, int empresaId)
    {
        if (contaReceberId <= 0 || empresaId <= 0) return Result<int>.Fail("Documento inválido.");
        try { var id = await _gateway.DuplicarAsync(contaReceberId, empresaId); return Result<int>.Ok(id, "Novo rascunho criado a partir do documento selecionado."); }
        catch (Exception ex) { return Result<int>.Fail(ex.Message); }
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
        try { await _gateway.AprovarPropostaAsync(contaReceberId); return Result.Ok("Fatura proforma validada."); }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> FaturarAsync(int contaReceberId)
    {
        if (contaReceberId <= 0) return Result.Fail("Proposta inválida.");
        try { await _gateway.FaturarAsync(contaReceberId); return Result.Ok("Fatura definitiva emitida e conta a receber ativada."); }
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
        if (!dto.CriarComoProposta && dto.Valor <= 0) erros.Add("O valor tem de ser maior do que zero.");
        if (dto.DescontoGeral < 0 || dto.Frete < 0 || dto.OutrasDespesas < 0) erros.Add("Desconto, frete e outras despesas não podem ser negativos.");
        if (!dto.CriarComoProposta && dto.DescontoGeral > dto.Valor + dto.Frete + dto.OutrasDespesas) erros.Add("O desconto não pode exceder o valor total do documento.");
        if (dto.DataEmissao == default) erros.Add("A data de emissão é obrigatória.");
        if (dto.DataVencimento == default) erros.Add("A data de vencimento é obrigatória.");
        if (dto.DataVencimento.Date < dto.DataEmissao.Date) erros.Add("A data de vencimento não pode ser anterior à data de emissão.");
        return erros;
    }

    public async Task<Result<IReadOnlyList<ProdutoStockDto>>> ListarProdutosAsync(int empresaId){ try{return Result<IReadOnlyList<ProdutoStockDto>>.Ok(await _gateway.ListarProdutosAsync(empresaId));}catch(Exception ex){return Result<IReadOnlyList<ProdutoStockDto>>.Fail(ex.Message);} }
    public async Task<Result<IReadOnlyList<DocumentoItemDto>>> ListarItensAsync(int documentoId){ try{return Result<IReadOnlyList<DocumentoItemDto>>.Ok(await _gateway.ListarItensAsync(documentoId));}catch(Exception ex){return Result<IReadOnlyList<DocumentoItemDto>>.Fail(ex.Message);} }
    public async Task<Result> AdicionarItemAsync(NovoDocumentoItemDto dto)
    {
        if (dto.DocumentoId <= 0 || dto.ProdutoId <= 0) return Result.Fail("Selecione a fatura proforma e o produto ou serviço.");
        if (dto.Quantidade <= 0) return Result.Fail("A quantidade deve ser maior do que zero.");
        if (dto.PrecoUnitario < 0) return Result.Fail("O preço unitário não pode ser negativo.");
        if (dto.DescontoPercentual is < 0 or > 100) return Result.Fail("O desconto deve estar entre 0% e 100%.");
        if (dto.IvaPercentual is < 0 or > 100) return Result.Fail("A taxa de IVA deve estar entre 0% e 100%.");
        try { await _gateway.AdicionarItemAsync(dto); return Result.Ok("Item adicionado e total da fatura recalculado."); }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }
    public async Task<Result> AtualizarItemAsync(AtualizarDocumentoItemDto dto)
    {
        if(dto.Id<=0||dto.DocumentoId<=0||dto.ProdutoId<=0)return Result.Fail("Item inválido.");
        if(dto.Quantidade<=0||dto.PrecoUnitario<0||dto.DescontoPercentual<0||dto.DescontoPercentual>100||dto.IvaPercentual<0||dto.IvaPercentual>100)return Result.Fail("Quantidade, preço, desconto ou IVA inválido.");
        try{await _gateway.AtualizarItemAsync(dto);return Result.Ok("Item atualizado e total recalculado.");}catch(Exception ex){return Result.Fail(ex.Message);}
    }
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
