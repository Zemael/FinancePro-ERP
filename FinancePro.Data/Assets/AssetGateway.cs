using FinancePro.Application.Assets;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.Assets;

public sealed class AssetGateway : IAssetGateway
{
    private readonly IBemService _service;

    public AssetGateway(IBemService service)
    {
        _service = service;
    }

    public Task<IReadOnlyList<BemListItemDto>> ListarAsync(int empresaId) =>
        _service.ListarAsync(empresaId);

    public Task<int> CriarAsync(NovoBemDto dto, int utilizadorId, string utilizadorNome) =>
        _service.CriarAsync(dto, utilizadorId, utilizadorNome);

    public Task AtualizarAsync(int bemId, NovoBemDto dto, int utilizadorId, string utilizadorNome) =>
        _service.AtualizarAsync(bemId, dto, utilizadorId, utilizadorNome);

    public Task AbaterAsync(int bemId, int utilizadorId, string utilizadorNome) =>
        _service.EliminarAsync(bemId, utilizadorId, utilizadorNome);
}
