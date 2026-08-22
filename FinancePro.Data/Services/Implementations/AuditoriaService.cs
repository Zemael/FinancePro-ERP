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

    public async Task<IReadOnlyList<AuditoriaConsultaDto>> ListarAsync(int empresaId, DateTime? de = null, DateTime? ate = null, string? termo = null)
    {
        var q = _context.LogsAuditoria.AsNoTracking().Where(x => x.EmpresaId == empresaId);
        if (de.HasValue) q = q.Where(x => x.Data >= de.Value.Date);
        if (ate.HasValue) q = q.Where(x => x.Data < ate.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(termo))
        {
            var t = termo.Trim();
            q = q.Where(x => x.Entidade.Contains(t) || x.Acao.Contains(t) || x.UtilizadorNome.Contains(t) || (x.Detalhe != null && x.Detalhe.Contains(t)));
        }
        return await q.OrderByDescending(x => x.Data).Take(1000).Select(x => new AuditoriaConsultaDto
        { Id=x.Id, Data=x.Data, Entidade=x.Entidade, RegistoId=x.RegistoId, Acao=x.Acao, Detalhe=x.Detalhe, UtilizadorId=x.UtilizadorId, UtilizadorNome=x.UtilizadorNome }).ToListAsync();
    }
}
