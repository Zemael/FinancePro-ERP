using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public sealed class MoedaService : IMoedaService
{
    private readonly FinanceProDbContext _context;
    public MoedaService(FinanceProDbContext context) => _context = context;

    public async Task<IReadOnlyList<MoedaDto>> ListarAsync(string? pesquisa = null)
    {
        var query = _context.Moedas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var termo = pesquisa.Trim();
            query = query.Where(x => x.CodigoIso.Contains(termo) || x.Nome.Contains(termo));
        }
        return await query.OrderBy(x => x.CodigoIso).Select(x => new MoedaDto
        {
            Id = x.Id, CodigoIso = x.CodigoIso, Nome = x.Nome, Simbolo = x.Simbolo,
            CasasDecimais = x.CasasDecimais, Ativo = x.Ativo
        }).ToListAsync();
    }

    public async Task<int> GuardarAsync(MoedaDto dto)
    {
        var codigo = (dto.CodigoIso ?? string.Empty).Trim().ToUpperInvariant();
        if (codigo.Length != 3) throw new InvalidOperationException("O código ISO deve ter exatamente 3 caracteres.");
        if (string.IsNullOrWhiteSpace(dto.Nome)) throw new InvalidOperationException("O nome da moeda é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Simbolo)) throw new InvalidOperationException("O símbolo da moeda é obrigatório.");
        if (dto.CasasDecimais is < 0 or > 4) throw new InvalidOperationException("Casas decimais deve estar entre 0 e 4.");

        var duplicada = await _context.Moedas.AnyAsync(x => x.Id != dto.Id && x.CodigoIso == codigo);
        if (duplicada) throw new InvalidOperationException("Já existe uma moeda com este código ISO.");

        Moeda entity;
        if (dto.Id == 0)
        {
            entity = new Moeda();
            _context.Moedas.Add(entity);
        }
        else entity = await _context.Moedas.FindAsync(dto.Id) ?? throw new InvalidOperationException("Moeda não encontrada.");

        entity.CodigoIso = codigo;
        entity.Nome = dto.Nome.Trim();
        entity.Simbolo = dto.Simbolo.Trim();
        entity.CasasDecimais = dto.CasasDecimais;
        entity.Ativo = dto.Ativo;
        entity.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task AlternarAtivoAsync(int id, bool ativo)
    {
        var entity = await _context.Moedas.FindAsync(id) ?? throw new InvalidOperationException("Moeda não encontrada.");
        entity.Ativo = ativo;
        entity.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
