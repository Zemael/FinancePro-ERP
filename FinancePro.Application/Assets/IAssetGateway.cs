using FinancePro.Core.DTOs;

namespace FinancePro.Application.Assets;

public interface IAssetGateway
{
    Task<IReadOnlyList<BemListItemDto>> ListarAsync(int empresaId);
    Task<int> CriarAsync(NovoBemDto dto, int utilizadorId, string utilizadorNome);
    Task AtualizarAsync(int bemId, NovoBemDto dto, int utilizadorId, string utilizadorNome);
    Task AbaterAsync(int bemId, int utilizadorId, string utilizadorNome);
}
