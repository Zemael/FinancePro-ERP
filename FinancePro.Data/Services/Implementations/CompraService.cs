using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Core.Enums;
using FinancePro.Data.Context;
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
        compra.DataRececao = DateTime.Today; compra.Estado = EstadoCompra.Recebido; compra.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
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
    }

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
