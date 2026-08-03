using FinancePro.Application.MasterData.Currencies;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.MasterData;

public sealed class CurrencyMasterDataGateway : ICurrencyMasterDataGateway
{
    private readonly IMoedaService _service;
    public CurrencyMasterDataGateway(IMoedaService service) => _service = service;
    public Task<IReadOnlyList<MoedaDto>> ListAsync(string? search, CancellationToken cancellationToken = default) => _service.ListarAsync(search);
    public Task<int> SaveAsync(MoedaDto currency, CancellationToken cancellationToken = default) => _service.GuardarAsync(currency);
    public Task SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default) => _service.AlternarAtivoAsync(id, active);
}
