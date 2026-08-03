using FinancePro.Application.MasterData.Banking;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.MasterData;

public sealed class BankingMasterDataServiceTests
{
    [Fact]
    public async Task CreateBankAsync_ShouldFail_WhenNameIsMissing()
    {
        var service = new BankingMasterDataService(new FakeGateway());
        var result = await service.CreateBankAsync("", null);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldNormalizeIbanAndCurrency()
    {
        var gateway = new FakeGateway();
        var service = new BankingMasterDataService(gateway);
        var result = await service.CreateAccountAsync(new NovaContaBancariaDto
        {
            EmpresaId = 1, BancoId = 1, NumeroConta = " 123 ", Titular = " Empresa ", IBAN = "gw 12 34", Moeda = " xof "
        });
        Assert.True(result.IsSuccess);
        Assert.Equal("GW1234", gateway.LastAccount?.IBAN);
        Assert.Equal("XOF", gateway.LastAccount?.Moeda);
    }

    private sealed class FakeGateway : IBankingMasterDataGateway
    {
        public NovaContaBancariaDto? LastAccount { get; private set; }
        public Task<IReadOnlyList<BancoListItemDto>> ListBanksAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<BancoListItemDto>>(Array.Empty<BancoListItemDto>());
        public Task CreateBankAsync(NovoBancoDto request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<ContaBancariaListItemDto>> ListAccountsAsync(int companyId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ContaBancariaListItemDto>>(Array.Empty<ContaBancariaListItemDto>());
        public Task CreateAccountAsync(NovaContaBancariaDto request, CancellationToken cancellationToken = default) { LastAccount = request; return Task.CompletedTask; }
        public Task SetAccountActiveAsync(int accountId, bool active, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
