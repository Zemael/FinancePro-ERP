using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public sealed class EmpresaService : IEmpresaService
{
    private readonly FinanceProDbContext _context;

    public EmpresaService(FinanceProDbContext context) => _context = context;

    public async Task<IReadOnlyList<EmpresaListItemDto>> ListarAsync(string? pesquisa = null)
    {
        var query = _context.Empresas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var termo = pesquisa.Trim();
            query = query.Where(e => e.Nome.Contains(termo) ||
                                     (e.NIF != null && e.NIF.Contains(termo)) ||
                                     (e.Email != null && e.Email.Contains(termo)));
        }

        return await query.OrderBy(e => e.Nome)
            .Select(e => new EmpresaListItemDto
            {
                Id = e.Id,
                Nome = e.Nome,
                NIF = e.NIF,
                Telefone = e.Telefone,
                Email = e.Email,
                Moeda = e.Moeda,
                Ativo = e.Ativo
            }).ToListAsync();
    }

    public async Task<EmpresaDto?> ObterAsync(int id) =>
        await _context.Empresas.AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EmpresaDto
            {
                Id = e.Id,
                Nome = e.Nome,
                NIF = e.NIF,
                Morada = e.Morada,
                Telefone = e.Telefone,
                Email = e.Email,
                Moeda = e.Moeda,
                Logotipo = e.Logotipo
            }).FirstOrDefaultAsync();

    public async Task<int> GuardarAsync(EmpresaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            throw new InvalidOperationException("O nome da empresa é obrigatório.");

        var nome = dto.Nome.Trim();
        var duplicada = await _context.Empresas.AnyAsync(e => e.Id != dto.Id && e.Nome == nome);
        if (duplicada)
            throw new InvalidOperationException("Já existe uma empresa com este nome.");

        Empresa empresa;
        if (dto.Id == 0)
        {
            empresa = new Empresa();
            _context.Empresas.Add(empresa);
        }
        else
        {
            empresa = await _context.Empresas.FindAsync(dto.Id)
                ?? throw new InvalidOperationException("Empresa não encontrada.");
        }

        empresa.Nome = nome;
        empresa.NIF = dto.NIF?.Trim();
        empresa.Morada = dto.Morada?.Trim();
        empresa.Telefone = dto.Telefone?.Trim();
        empresa.Email = dto.Email?.Trim();
        empresa.Moeda = string.IsNullOrWhiteSpace(dto.Moeda) ? "FCFA" : dto.Moeda.Trim().ToUpperInvariant();
        empresa.Logotipo = dto.Logotipo;
        empresa.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return empresa.Id;
    }

    public async Task AlternarAtivoAsync(int id, bool ativo)
    {
        var empresa = await _context.Empresas.FindAsync(id)
            ?? throw new InvalidOperationException("Empresa não encontrada.");
        empresa.Ativo = ativo;
        empresa.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
