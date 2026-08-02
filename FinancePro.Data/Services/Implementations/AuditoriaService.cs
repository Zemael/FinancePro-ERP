using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class AuditoriaService : IAuditoriaService
{
    private readonly FinanceProDbContext _context;

    public AuditoriaService(FinanceProDbContext context)
    {
        _context = context;
    }

    public async Task RegistarAsync(string entidade, int registoId, string acao, string? detalhe, int utilizadorId, string utilizadorNome, int empresaId)
    {
        _context.LogsAuditoria.Add(new LogAuditoria
        {
            Entidade = entidade,
            RegistoId = registoId,
            Acao = acao,
            Detalhe = detalhe,
            UtilizadorId = utilizadorId,
            UtilizadorNome = utilizadorNome,
            EmpresaId = empresaId
        });

        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<LogAuditoriaDto>> ListarPorRegistoAsync(string entidade, int registoId)
    {
        return await _context.LogsAuditoria
            .Where(l => l.Entidade == entidade && l.RegistoId == registoId)
            .OrderByDescending(l => l.Data)
            .Select(l => new LogAuditoriaDto
            {
                Data = l.Data,
                Acao = l.Acao,
                Detalhe = l.Detalhe,
                UtilizadorNome = l.UtilizadorNome
            })
            .ToListAsync();
    }
}
