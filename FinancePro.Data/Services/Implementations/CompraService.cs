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
            PodeAprovar = c.Estado == EstadoCompra.Pendente,
            PodeRejeitar = c.Estado == EstadoCompra.Pendente,
            PodeCancelar = c.Estado is EstadoCompra.Pendente or EstadoCompra.Aprovado
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

    public Task AprovarAsync(int compraId) => MudarEstadoAsync(compraId, EstadoCompra.Pendente, EstadoCompra.Aprovado);

    public Task RejeitarAsync(int compraId) => MudarEstadoAsync(compraId, EstadoCompra.Pendente, EstadoCompra.Rejeitado);

    public async Task CancelarAsync(int compraId)
    {
        var compra = await _context.Compras.FindAsync(compraId)
            ?? throw new InvalidOperationException("Compra não encontrada.");

        if (compra.Estado is not (EstadoCompra.Pendente or EstadoCompra.Aprovado))
        {
            throw new InvalidOperationException("Só é possível cancelar compras pendentes ou aprovadas.");
        }

        compra.Estado = EstadoCompra.Cancelado;
        compra.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

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
