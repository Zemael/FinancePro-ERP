using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public sealed class ExercicioFinanceiroService : IExercicioFinanceiroService
{
    private readonly FinanceProDbContext _context;
    public ExercicioFinanceiroService(FinanceProDbContext context) => _context = context;

    public async Task<IReadOnlyList<ExercicioFinanceiroDto>> ListarAsync(string? pesquisa = null)
    {
        var query = _context.ExerciciosFinanceiros.AsNoTracking().Include(x => x.Empresa).AsQueryable();
        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var termo = pesquisa.Trim();
            query = query.Where(x => x.Ano.ToString().Contains(termo) || x.Empresa.Nome.Contains(termo));
        }

        return await query.OrderByDescending(x => x.Ano).ThenBy(x => x.Empresa.Nome)
            .Select(x => new ExercicioFinanceiroDto
            {
                Id = x.Id, EmpresaId = x.EmpresaId, EmpresaNome = x.Empresa.Nome,
                Ano = x.Ano, DataInicio = x.DataInicio, DataFim = x.DataFim,
                Padrao = x.Padrao, Encerrado = x.Encerrado, Ativo = x.Ativo
            }).ToListAsync();
    }

    public async Task<int> GuardarAsync(ExercicioFinanceiroDto dto)
    {
        if (dto.EmpresaId <= 0) throw new InvalidOperationException("Selecione uma empresa.");
        if (dto.Ano < 2000 || dto.Ano > 2200) throw new InvalidOperationException("Informe um ano válido.");
        if (dto.DataFim < dto.DataInicio) throw new InvalidOperationException("A data final não pode ser anterior à inicial.");

        var duplicado = await _context.ExerciciosFinanceiros.AnyAsync(x => x.Id != dto.Id && x.EmpresaId == dto.EmpresaId && x.Ano == dto.Ano);
        if (duplicado) throw new InvalidOperationException("Já existe este exercício para a empresa selecionada.");

        ExercicioFinanceiro entity;
        if (dto.Id == 0)
        {
            entity = new ExercicioFinanceiro();
            _context.ExerciciosFinanceiros.Add(entity);
        }
        else entity = await _context.ExerciciosFinanceiros.FindAsync(dto.Id) ?? throw new InvalidOperationException("Exercício não encontrado.");

        if (dto.Padrao)
        {
            var outros = await _context.ExerciciosFinanceiros.Where(x => x.EmpresaId == dto.EmpresaId && x.Id != dto.Id && x.Padrao).ToListAsync();
            foreach (var item in outros) item.Padrao = false;
        }

        entity.EmpresaId = dto.EmpresaId;
        entity.Ano = dto.Ano;
        entity.DataInicio = dto.DataInicio;
        entity.DataFim = dto.DataFim;
        entity.Padrao = dto.Padrao;
        entity.Encerrado = dto.Encerrado;
        entity.Ativo = dto.Ativo;
        entity.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task AlternarAtivoAsync(int id, bool ativo)
    {
        var entity = await _context.ExerciciosFinanceiros.FindAsync(id) ?? throw new InvalidOperationException("Exercício não encontrado.");
        entity.Ativo = ativo;
        entity.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
