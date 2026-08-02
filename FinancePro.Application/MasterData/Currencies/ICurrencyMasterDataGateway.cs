using FinancePro.Core.DTOs;

namespace FinancePro.Application.MasterData.Currencies;

public interface ICurrencyMasterDataGateway
{
    Task<IReadOnlyList<MoedaDto>> ListAsync(string? search, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(MoedaDto currency, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default);
}
