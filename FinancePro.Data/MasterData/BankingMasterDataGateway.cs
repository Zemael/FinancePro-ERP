using FinancePro.Application.MasterData.Banking;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.MasterData;

public sealed class BankingMasterDataGateway : IBankingMasterDataGateway
{
    private readonly IBancoService _service;
    public BankingMasterDataGateway(IBancoService service) => _service = service;

    public Task<IReadOnlyList<BancoListItemDto>> ListBanksAsync(CancellationToken cancellationToken = default) => _service.ListarBancosAsync();
    public Task CreateBankAsync(NovoBancoDto request, CancellationToken cancellationToken = default) => _service.CriarBancoAsync(request);
    public Task UpdateBankAsync(int bankId, NovoBancoDto request, CancellationToken cancellationToken = default) => _service.AtualizarBancoAsync(bankId, request);
    public Task<IReadOnlyList<ContaBancariaListItemDto>> ListAccountsAsync(int companyId, CancellationToken cancellationToken = default) => _service.ListarContasAsync(companyId);
    public Task CreateAccountAsync(NovaContaBancariaDto request, CancellationToken cancellationToken = default) => _service.CriarContaAsync(request);
    public Task UpdateAccountAsync(int accountId, NovaContaBancariaDto request, CancellationToken cancellationToken = default) => _service.AtualizarContaAsync(accountId, request);
    public Task SetAccountActiveAsync(int accountId, bool active, CancellationToken cancellationToken = default) => _service.AlternarAtivoContaAsync(accountId, active);
}
