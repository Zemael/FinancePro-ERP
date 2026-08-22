using FinancePro.Core.DTOs;

namespace FinancePro.Application.MasterData.Banking;

public interface IBankingMasterDataGateway
{
    Task<IReadOnlyList<BancoListItemDto>> ListBanksAsync(CancellationToken cancellationToken = default);
    Task CreateBankAsync(NovoBancoDto request, CancellationToken cancellationToken = default);
    Task UpdateBankAsync(int bankId, NovoBancoDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContaBancariaListItemDto>> ListAccountsAsync(int companyId, CancellationToken cancellationToken = default);
    Task CreateAccountAsync(NovaContaBancariaDto request, CancellationToken cancellationToken = default);
    Task UpdateAccountAsync(int accountId, NovaContaBancariaDto request, CancellationToken cancellationToken = default);
    Task SetAccountActiveAsync(int accountId, bool active, CancellationToken cancellationToken = default);
}
