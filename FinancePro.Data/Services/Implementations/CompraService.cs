using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Core.Enums;
using FinancePro.Data.Context;
using FinancePro.Data.Accounting;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class CompraService : ICompraService
{
    private readonly FinanceProDbContext _context;

    public CompraService(FinanceProDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CompraListItemDto>> ListarAsync(int empresaId)
    {
        var compras = await _context.Compras
            .Where(c => c.EmpresaId == empresaId)
            .Include(c => c.Fornecedor)
            .OrderByDescending(c => c.Data)
            .ToListAsync();

        return compras.Select(c => new CompraListItemDto
        {
            Id = c.Id,
            NumeroPedido = c.NumeroPedido,
            FornecedorNome = c.Fornecedor?.Nome,
            Data = c.Data,
            Departamento = c.Departamento,
            CentroCusto = c.CentroCusto,
            Comprador = c.Comprador,
            Prioridade = c.Prioridade.ToString(),
            Estado = c.Estado.ToString(),
            ValorTotal = c.ValorTotal,
            NumeroCotacao = c.NumeroCotacao,
            NumeroOrdemCompra = c.NumeroOrdemCompra,
            PodeCotar = c.Estado == EstadoCompra.Pendente,
            PodeAprovar = c.Estado == EstadoCompra.Cotado,
            PodeRejeitar = c.Estado is EstadoCompra.Pendente or EstadoCompra.Cotado,
            PodeCancelar = c.Estado is EstadoCompra.Pendente or EstadoCompra.Cotado or EstadoCompra.Aprovado or EstadoCompra.OrdemEmitida,
            PodeEmitirOrdem = c.Estado == EstadoCompra.Aprovado,
            PodeReceber = c.Estado == EstadoCompra.OrdemEmitida,
            PodeFaturar = c.Estado == EstadoCompra.Recebido
        }).ToList();
    }

    public async Task<IReadOnlyList<FornecedorOpcaoDto>> ListarFornecedoresAsync(int empresaId)
    {
        return await _context.Fornecedores
            .Where(f => f.EmpresaId == empresaId && f.Ativo)
            .OrderBy(f => f.Nome)
            .Select(f => new FornecedorOpcaoDto { Id = f.Id, Nome = f.Nome })
            .ToListAsync();
    }

    public async Task CriarAsync(NovaCompraDto dto)
    {
        if (dto.ValorTotal < 0)
        {
            throw new InvalidOperationException("O valor total não pode ser negativo.");
        }

        var proximoNumero = 1 + await _context.Compras.CountAsync(c => c.EmpresaId == dto.EmpresaId);

        _context.Compras.Add(new Compra
        {
            NumeroPedido = $"COMP-{proximoNumero:D5}",
            Data = dto.Data,
            Departamento = dto.Departamento,
            CentroCusto = dto.CentroCusto,
            Projeto = dto.Projeto,
            Comprador = dto.Comprador,
            Prioridade = dto.Prioridade,
            ValorTotal = dto.ValorTotal,
            FornecedorId = dto.FornecedorId,
            EmpresaId = dto.EmpresaId,
            Estado = EstadoCompra.Pendente
        });

        await _context.SaveChangesAsync();
    }

    public async Task CotarAsync(int compraId)
    {
        var compra = await ObterAsync(compraId);
        if (compra.Estado != EstadoCompra.Pendente) throw new InvalidOperationException("Só pedidos pendentes podem receber cotação.");
        compra.NumeroCotacao = $"COT-{compra.Id:D6}"; compra.DataCotacao = DateTime.Today; compra.Estado = EstadoCompra.Cotado;
        compra.DataAtualizacao = DateTime.UtcNow; await _context.SaveChangesAsync();
    }

    public Task AprovarAsync(int compraId) => MudarEstadoAsync(compraId, EstadoCompra.Cotado, EstadoCompra.Aprovado);

    public async Task RejeitarAsync(int compraId)
    {
        var compra = await ObterAsync(compraId);
        if (compra.Estado is not (EstadoCompra.Pendente or EstadoCompra.Cotado)) throw new InvalidOperationException("Só pedidos pendentes ou cotados podem ser rejeitados.");
        compra.Estado = EstadoCompra.Rejeitado; compra.DataAtualizacao = DateTime.UtcNow; await _context.SaveChangesAsync();
    }

    public async Task CancelarAsync(int compraId)
    {
        var compra = await _context.Compras.FindAsync(compraId)
            ?? throw new InvalidOperationException("Compra não encontrada.");

        if (compra.Estado is not (EstadoCompra.Pendente or EstadoCompra.Cotado or EstadoCompra.Aprovado or EstadoCompra.OrdemEmitida))
        {
            throw new InvalidOperationException("Só é possível cancelar compras pendentes ou aprovadas.");
        }

        compra.Estado = EstadoCompra.Cancelado;
        compra.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }


    public async Task EmitirOrdemAsync(int compraId)
    {
        var compra = await ObterAsync(compraId);
        if (compra.Estado != EstadoCompra.Aprovado) throw new InvalidOperationException("A compra precisa estar aprovada.");
        compra.NumeroOrdemCompra = $"OC-{compra.Id:D6}"; compra.DataOrdemCompra = DateTime.Today; compra.Estado = EstadoCompra.OrdemEmitida;
        compra.DataAtualizacao = DateTime.UtcNow; await _context.SaveChangesAsync();
    }

    public async Task ReceberAsync(int compraId)
    {
        var compra = await ObterAsync(compraId);
        if (compra.Estado != EstadoCompra.OrdemEmitida) throw new InvalidOperationException("Só ordens emitidas podem ser recebidas.");
        var itens = await _context.CompraItens.Include(x=>x.Produto).Where(x=>x.CompraId==compra.Id).ToListAsync();
        if (itens.Count == 0) throw new InvalidOperationException("Adicione pelo menos um produto à ordem antes da receção.");
        await using var tx = await _context.Database.BeginTransactionAsync();
        foreach(var i in itens){ var referencia=compra.NumeroOrdemCompra ?? compra.NumeroPedido; if(await _context.MovimentosStock.AnyAsync(x=>x.ProdutoId==i.ProdutoId && x.DocumentoReferencia==referencia && x.Tipo==TipoMovimentoStock.Entrada)) continue; var novo=i.Produto.StockAtual+i.Quantidade; i.Produto.CustoMedio=novo==0?0:((i.Produto.StockAtual*i.Produto.CustoMedio)+(i.Quantidade*i.PrecoUnitario))/novo; i.Produto.StockAtual=novo; i.Produto.DataAtualizacao=DateTime.UtcNow; _context.MovimentosStock.Add(new MovimentoStock{EmpresaId=compra.EmpresaId,ProdutoId=i.ProdutoId,Tipo=TipoMovimentoStock.Entrada,Quantidade=i.Quantidade,CustoUnitario=i.PrecoUnitario,SaldoApos=i.Produto.StockAtual,DocumentoReferencia=referencia,Observacao="Receção automática de compra"}); }
        compra.DataRececao = DateTime.Today; compra.Estado = EstadoCompra.Recebido; compra.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync(); await tx.CommitAsync();
    }

    public async Task FaturarAsync(int compraId)
    {
        var compra = await _context.Compras.Include(c => c.Fornecedor).FirstOrDefaultAsync(c => c.Id == compraId)
            ?? throw new InvalidOperationException("Compra não encontrada.");
        if (compra.Estado != EstadoCompra.Recebido) throw new InvalidOperationException("A compra precisa estar recebida antes da faturação.");
        var codigo = $"CP-OC-{compra.Id:D6}";
        if (!await _context.ContasPagar.AnyAsync(x => x.EmpresaId == compra.EmpresaId && x.Codigo == codigo))
        {
            var prazo = compra.PrazoPagamentoDias > 0 ? compra.PrazoPagamentoDias : (compra.Fornecedor?.PrazoPagamentoDias ?? 30);
            _context.ContasPagar.Add(new ContaPagar { Codigo = codigo, Descricao = $"Fatura da ordem {compra.NumeroOrdemCompra}", Valor = compra.ValorTotal, ValorLiquidado = 0, DataEmissao = DateTime.Today, DataVencimento = DateTime.Today.AddDays(prazo), Estado = EstadoConta.Pendente, CentroCusto = compra.CentroCusto, FornecedorId = compra.FornecedorId, EmpresaId = compra.EmpresaId });
        }
        compra.DataFatura = DateTime.Today; compra.Estado = EstadoCompra.Faturado; compra.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await AutomaticAccountingPoster.TryPostAsync(_context, compra.EmpresaId, "COMPRA_FATURA", compra.Id, compra.DataFatura.Value, codigo, compra.NumeroOrdemCompra ?? compra.NumeroPedido, $"Fatura de compra {compra.NumeroOrdemCompra ?? compra.NumeroPedido}", compra.ValorTotal);
    }


    public async Task<IReadOnlyList<ProdutoStockDto>> ListarProdutosAsync(int empresaId) => await _context.Produtos.AsNoTracking().Where(x=>x.EmpresaId==empresaId && x.Ativo).OrderBy(x=>x.Nome).Select(x=>new ProdutoStockDto{Id=x.Id,Codigo=x.Codigo,Nome=x.Nome,Unidade=x.Unidade,StockAtual=x.StockAtual,CustoMedio=x.CustoMedio}).ToListAsync();
    public async Task<IReadOnlyList<DocumentoItemDto>> ListarItensAsync(int compraId) => await _context.CompraItens.AsNoTracking().Include(x=>x.Produto).Where(x=>x.CompraId==compraId).OrderBy(x=>x.Id).Select(x=>new DocumentoItemDto{Id=x.Id,ProdutoId=x.ProdutoId,ProdutoCodigo=x.Produto.Codigo,ProdutoNome=x.Produto.Nome,Unidade=x.Produto.Unidade,Quantidade=x.Quantidade,PrecoUnitario=x.PrecoUnitario,DescontoPercentual=x.DescontoPercentual,IvaPercentual=x.IvaPercentual,Subtotal=x.Quantidade*x.PrecoUnitario*(1-x.DescontoPercentual/100m),ValorIva=x.Quantidade*x.PrecoUnitario*(1-x.DescontoPercentual/100m)*x.IvaPercentual/100m,Total=x.Quantidade*x.PrecoUnitario*(1-x.DescontoPercentual/100m)*(1+x.IvaPercentual/100m)}).ToListAsync();
    public async Task AdicionarItemAsync(NovoDocumentoItemDto d){ var compra=await _context.Compras.Include(x=>x.Itens).FirstOrDefaultAsync(x=>x.Id==d.DocumentoId)??throw new InvalidOperationException("Compra não encontrada."); if(compra.Estado is EstadoCompra.Recebido or EstadoCompra.Faturado or EstadoCompra.Cancelado) throw new InvalidOperationException("Não é possível alterar itens nesta fase da compra."); if(d.Quantidade<=0||d.PrecoUnitario<0||d.DescontoPercentual<0||d.DescontoPercentual>100||d.IvaPercentual<0) throw new InvalidOperationException("Valores do item inválidos."); if(!await _context.Produtos.AnyAsync(x=>x.Id==d.ProdutoId&&x.EmpresaId==compra.EmpresaId&&x.Ativo)) throw new InvalidOperationException("Produto inválido."); var item=new CompraItem{CompraId=compra.Id,ProdutoId=d.ProdutoId,Quantidade=d.Quantidade,PrecoUnitario=d.PrecoUnitario,DescontoPercentual=d.DescontoPercentual,IvaPercentual=d.IvaPercentual}; _context.CompraItens.Add(item); await _context.SaveChangesAsync(); await RecalcularCompraAsync(compra.Id); }
    public async Task RemoverItemAsync(int itemId){ var item=await _context.CompraItens.Include(x=>x.Compra).FirstOrDefaultAsync(x=>x.Id==itemId)??throw new InvalidOperationException("Item não encontrado."); if(item.Compra.Estado is EstadoCompra.Recebido or EstadoCompra.Faturado or EstadoCompra.Cancelado) throw new InvalidOperationException("Não é possível remover itens nesta fase."); var id=item.CompraId; _context.CompraItens.Remove(item); await _context.SaveChangesAsync(); await RecalcularCompraAsync(id); }
    private async Task RecalcularCompraAsync(int id){ var c=await _context.Compras.FindAsync(id)??throw new InvalidOperationException("Compra não encontrada."); var itens=await _context.CompraItens.Where(x=>x.CompraId==id).ToListAsync(); c.ValorTotal=itens.Sum(x=>x.Total); c.DataAtualizacao=DateTime.UtcNow; await _context.SaveChangesAsync(); }

    private async Task<Compra> ObterAsync(int compraId) => await _context.Compras.FindAsync(compraId)
        ?? throw new InvalidOperationException("Compra não encontrada.");

    private async Task MudarEstadoAsync(int compraId, EstadoCompra estadoEsperado, EstadoCompra novoEstado)
    {
        var compra = await _context.Compras.FindAsync(compraId)
            ?? throw new InvalidOperationException("Compra não encontrada.");

        if (compra.Estado != estadoEsperado)
        {
            throw new InvalidOperationException($"Só é possível fazer esta ação quando a compra está {estadoEsperado}.");
        }

        compra.Estado = novoEstado;
        compra.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
