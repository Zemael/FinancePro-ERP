using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public sealed class PerfilService : IPerfilService
{
    private readonly FinanceProDbContext _context;
    public PerfilService(FinanceProDbContext context) => _context = context;

    public async Task<IReadOnlyList<PerfilDto>> ListarAsync(string? pesquisa = null)
    {
        var query = _context.Perfis.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var termo = pesquisa.Trim();
            query = query.Where(x => x.Nome.Contains(termo) || (x.Descricao ?? "").Contains(termo));
        }

        return await query.OrderBy(x => x.Nome).Select(x => new PerfilDto
        {
            Id = x.Id,
            Nome = x.Nome,
            Descricao = x.Descricao,
            Ativo = x.Ativo
        }).ToListAsync();
    }

    public async Task<int> GuardarAsync(PerfilDto dto)
    {
        var nome = (dto.Nome ?? string.Empty).Trim();
        if (nome.Length < 3) throw new InvalidOperationException("O nome do perfil deve ter pelo menos 3 caracteres.");
        if (await _context.Perfis.AnyAsync(x => x.Id != dto.Id && x.Nome == nome))
            throw new InvalidOperationException("Já existe um perfil com este nome.");

        Perfil entity;
        if (dto.Id == 0)
        {
            entity = new Perfil();
            _context.Perfis.Add(entity);
        }
        else entity = await _context.Perfis.FindAsync(dto.Id) ?? throw new InvalidOperationException("Perfil não encontrado.");

        entity.Nome = nome;
        entity.Descricao = string.IsNullOrWhiteSpace(dto.Descricao) ? null : dto.Descricao.Trim();
        entity.Ativo = dto.Ativo;
        entity.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task AlternarAtivoAsync(int id, bool ativo)
    {
        var entity = await _context.Perfis.FindAsync(id) ?? throw new InvalidOperationException("Perfil não encontrado.");
        entity.Ativo = ativo;
        entity.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
