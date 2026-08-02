using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public sealed class UtilizadorService : IUtilizadorService
{
    private readonly FinanceProDbContext _context;
    public UtilizadorService(FinanceProDbContext context) => _context = context;

    public async Task<IReadOnlyList<UtilizadorDto>> ListarAsync(string? pesquisa = null)
    {
        IQueryable<Utilizador> query = _context.Utilizadores.AsNoTracking().Include(x => x.Perfil).Include(x => x.Empresa);
        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var termo = pesquisa.Trim();
            query = query.Where(x => x.NomeCompleto.Contains(termo) || x.Email.Contains(termo) || x.Perfil.Nome.Contains(termo));
        }

        return await query.OrderBy(x => x.NomeCompleto).Select(x => new UtilizadorDto
        {
            Id = x.Id,
            NomeCompleto = x.NomeCompleto,
            Email = x.Email,
            PerfilId = x.PerfilId,
            PerfilNome = x.Perfil.Nome,
            EmpresaId = x.EmpresaId,
            EmpresaNome = x.Empresa.Nome,
            UltimoLogin = x.UltimoLogin,
            Ativo = x.Ativo
        }).ToListAsync();
    }

    public async Task<int> GuardarAsync(UtilizadorDto dto)
    {
        var nome = (dto.NomeCompleto ?? string.Empty).Trim();
        var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (nome.Length < 3) throw new InvalidOperationException("O nome completo é obrigatório.");
        if (!email.Contains('@')) throw new InvalidOperationException("Indique um email válido.");
        if (!await _context.Perfis.AnyAsync(x => x.Id == dto.PerfilId && x.Ativo)) throw new InvalidOperationException("Selecione um perfil ativo.");
        if (!await _context.Empresas.AnyAsync(x => x.Id == dto.EmpresaId && x.Ativo)) throw new InvalidOperationException("Selecione uma empresa ativa.");
        if (await _context.Utilizadores.AnyAsync(x => x.Id != dto.Id && x.Email == email)) throw new InvalidOperationException("Já existe um utilizador com este email.");

        Utilizador entity;
        if (dto.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(dto.NovaPassword) || dto.NovaPassword.Length < 6)
                throw new InvalidOperationException("A palavra-passe inicial deve ter pelo menos 6 caracteres.");
            entity = new Utilizador { PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaPassword) };
            _context.Utilizadores.Add(entity);
        }
        else
        {
            entity = await _context.Utilizadores.FindAsync(dto.Id) ?? throw new InvalidOperationException("Utilizador não encontrado.");
            if (!string.IsNullOrWhiteSpace(dto.NovaPassword))
            {
                if (dto.NovaPassword.Length < 6) throw new InvalidOperationException("A nova palavra-passe deve ter pelo menos 6 caracteres.");
                entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaPassword);
            }
        }

        entity.NomeCompleto = nome;
        entity.Email = email;
        entity.PerfilId = dto.PerfilId;
        entity.EmpresaId = dto.EmpresaId;
        entity.Ativo = dto.Ativo;
        entity.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task AlternarAtivoAsync(int id, bool ativo)
    {
        var entity = await _context.Utilizadores.FindAsync(id) ?? throw new InvalidOperationException("Utilizador não encontrado.");
        entity.Ativo = ativo;
        entity.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
