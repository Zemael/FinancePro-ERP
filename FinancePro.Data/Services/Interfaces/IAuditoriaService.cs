using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IAuditoriaService
{
    Task RegistarAsync(string entidade, int registoId, string acao, string? detalhe, int utilizadorId, string utilizadorNome, int empresaId);
    Task<IReadOnlyList<LogAuditoriaDto>> ListarPorRegistoAsync(string entidade, int registoId);
    Task<IReadOnlyList<AuditoriaConsultaDto>> ListarAsync(int empresaId, DateTime? de = null, DateTime? ate = null, string? termo = null);
}
