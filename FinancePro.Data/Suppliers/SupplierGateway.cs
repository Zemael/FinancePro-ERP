using FinancePro.Application.Suppliers;
using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Suppliers;

public sealed class SupplierGateway : ISupplierGateway
{
    private readonly FinanceProDbContext _db;
    public SupplierGateway(FinanceProDbContext db) => _db = db;

    public async Task<IReadOnlyList<FornecedorDto>> ListAsync(int empresaId, string? pesquisa, CancellationToken cancellationToken = default)
    {
        var query = _db.Fornecedores.AsNoTracking().Where(x => x.EmpresaId == empresaId);
        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var p = pesquisa.Trim();
            query = query.Where(x => x.Nome.Contains(p) || (x.NIF != null && x.NIF.Contains(p)) || (x.Email != null && x.Email.Contains(p)));
        }
        return await query.OrderBy(x => x.Nome).Select(x => new FornecedorDto
        {
            Id=x.Id, Nome=x.Nome, NIF=x.NIF, Telefone=x.Telefone, Email=x.Email, Morada=x.Morada, Ativo=x.Ativo
        }).ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsWithNifAsync(int empresaId, string nif, int ignoreId, CancellationToken cancellationToken = default) =>
        _db.Fornecedores.AnyAsync(x => x.EmpresaId == empresaId && x.NIF == nif && x.Id != ignoreId, cancellationToken);

    public async Task<int> SaveAsync(SupplierSaveRequest r, CancellationToken cancellationToken = default)
    {
        Fornecedor entity;
        if (r.Id == 0) { entity = new Fornecedor { EmpresaId = r.EmpresaId }; _db.Fornecedores.Add(entity); }
        else { entity = await _db.Fornecedores.SingleAsync(x => x.Id == r.Id && x.EmpresaId == r.EmpresaId, cancellationToken); entity.DataAtualizacao = DateTime.UtcNow; }
        entity.Nome=r.Nome; entity.NIF=r.Nif; entity.Telefone=r.Telefone; entity.Email=r.Email; entity.Morada=r.Morada; entity.Ativo=r.Ativo;
        await _db.SaveChangesAsync(cancellationToken); return entity.Id;
    }

    public async Task SetActiveAsync(int id, bool ativo, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Fornecedores.SingleAsync(x => x.Id == id, cancellationToken);
        entity.Ativo=ativo; entity.DataAtualizacao=DateTime.UtcNow; await _db.SaveChangesAsync(cancellationToken);
    }
}
