using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardResumoDto> ObterResumoAsync(int empresaId, int utilizadorId);
    Task<IReadOnlyList<PesquisaResultadoDto>> PesquisarAsync(int empresaId, string termo);
}
