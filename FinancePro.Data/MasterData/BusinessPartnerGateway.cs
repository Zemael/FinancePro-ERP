using FinancePro.Application.MasterData.Partners;
using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.MasterData;

public sealed class BusinessPartnerGateway : IBusinessPartnerGateway
{
    private readonly FinanceProDbContext _db;
    public BusinessPartnerGateway(FinanceProDbContext db) => _db = db;

    public async Task<IReadOnlyList<BusinessPartnerDto>> ListAsync(int empresaId, string? tipo, string? pesquisa, CancellationToken cancellationToken = default)
    {
        var result = new List<BusinessPartnerDto>();
        var term = pesquisa?.Trim();
        if (tipo is null or "Cliente")
        {
            var q = _db.Clientes.AsNoTracking().Where(x => x.EmpresaId == empresaId);
            if (!string.IsNullOrWhiteSpace(term)) q = q.Where(x => x.Nome.Contains(term) || (x.NIF != null && x.NIF.Contains(term)) || (x.Email != null && x.Email.Contains(term)));
            result.AddRange(await q.OrderBy(x => x.Nome).Select(x => new BusinessPartnerDto { Id=x.Id, Tipo="Cliente", Nome=x.Nome, NIF=x.NIF, Telefone=x.Telefone, Email=x.Email, Morada=x.Morada, Ativo=x.Ativo }).ToListAsync(cancellationToken));
        }
        if (tipo is null or "Fornecedor")
        {
            var q = _db.Fornecedores.AsNoTracking().Where(x => x.EmpresaId == empresaId);
            if (!string.IsNullOrWhiteSpace(term)) q = q.Where(x => x.Nome.Contains(term) || (x.NIF != null && x.NIF.Contains(term)) || (x.Email != null && x.Email.Contains(term)));
            result.AddRange(await q.OrderBy(x => x.Nome).Select(x => new BusinessPartnerDto { Id=x.Id, Tipo="Fornecedor", Nome=x.Nome, NIF=x.NIF, Telefone=x.Telefone, Email=x.Email, Morada=x.Morada, Ativo=x.Ativo }).ToListAsync(cancellationToken));
        }
        return result.OrderBy(x => x.Tipo).ThenBy(x => x.Nome).ToList();
    }

    public async Task<BusinessPartnerDto?> GetAsync(int empresaId, string tipo, int id, CancellationToken cancellationToken = default)
    {
        if (tipo == "Cliente")
        {
            var x = await _db.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.EmpresaId == empresaId && x.Id == id, cancellationToken);
            return x is null ? null : MapCliente(x);
        }
        var f = await _db.Fornecedores.AsNoTracking().FirstOrDefaultAsync(x => x.EmpresaId == empresaId && x.Id == id, cancellationToken);
        return f is null ? null : MapFornecedor(f);
    }

    public async Task<int> SaveAsync(BusinessPartnerSaveRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Tipo == "Cliente")
        {
            var entity = request.Id == 0 ? new Cliente { EmpresaId = request.EmpresaId } : await _db.Clientes.FirstAsync(x => x.Id == request.Id && x.EmpresaId == request.EmpresaId, cancellationToken);
            entity.Nome=request.Nome; entity.NIF=request.NIF; entity.Telefone=request.Telefone; entity.Email=request.Email; entity.Morada=request.Morada; entity.DataAtualizacao=DateTime.UtcNow;
            if (request.Id == 0) _db.Clientes.Add(entity);
            await _db.SaveChangesAsync(cancellationToken); return entity.Id;
        }
        else
        {
            var entity = request.Id == 0 ? new Fornecedor { EmpresaId = request.EmpresaId } : await _db.Fornecedores.FirstAsync(x => x.Id == request.Id && x.EmpresaId == request.EmpresaId, cancellationToken);
            entity.Nome=request.Nome; entity.NIF=request.NIF; entity.Telefone=request.Telefone; entity.Email=request.Email; entity.Morada=request.Morada; entity.DataAtualizacao=DateTime.UtcNow;
            if (request.Id == 0) _db.Fornecedores.Add(entity);
            await _db.SaveChangesAsync(cancellationToken); return entity.Id;
        }
    }

    public async Task SetActiveAsync(int empresaId, string tipo, int id, bool ativo, CancellationToken cancellationToken = default)
    {
        EntityBase entity = tipo == "Cliente"
            ? await _db.Clientes.FirstAsync(x => x.Id == id && x.EmpresaId == empresaId, cancellationToken)
            : await _db.Fornecedores.FirstAsync(x => x.Id == id && x.EmpresaId == empresaId, cancellationToken);
        entity.Ativo = ativo; entity.DataAtualizacao = DateTime.UtcNow; await _db.SaveChangesAsync(cancellationToken);
    }

    private static BusinessPartnerDto MapCliente(Cliente x) => new() { Id=x.Id, Tipo="Cliente", Nome=x.Nome, NIF=x.NIF, Telefone=x.Telefone, Email=x.Email, Morada=x.Morada, Ativo=x.Ativo };
    private static BusinessPartnerDto MapFornecedor(Fornecedor x) => new() { Id=x.Id, Tipo="Fornecedor", Nome=x.Nome, NIF=x.NIF, Telefone=x.Telefone, Email=x.Email, Morada=x.Morada, Ativo=x.Ativo };
}
