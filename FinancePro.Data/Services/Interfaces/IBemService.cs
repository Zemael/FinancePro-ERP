using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IBemService
{
    Task<IReadOnlyList<BemListItemDto>> ListarAsync(int empresaId);
    Task<int> CriarAsync(NovoBemDto dto, int utilizadorId, string utilizadorNome);
    Task AtualizarAsync(int bemId, NovoBemDto dto, int utilizadorId, string utilizadorNome);
    Task EliminarAsync(int bemId, int utilizadorId, string utilizadorNome);
}
