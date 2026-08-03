using FinancePro.Application.MasterData.Currencies;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.MasterData;

public sealed class CurrencyMasterDataServiceTests
{
    [Fact]
    public async Task SaveAsync_ShouldFail_WhenIsoCodeIsInvalid()
    {
        var service = new CurrencyMasterDataService(new FakeGateway());
        var result = await service.SaveAsync(new MoedaDto { CodigoIso = "FC", Nome = "Franco CFA", CasasDecimais = 0 });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SaveAsync_ShouldNormalizeIsoCode()
    {
        var gateway = new FakeGateway();
        var service = new CurrencyMasterDataService(gateway);
        var result = await service.SaveAsync(new MoedaDto { CodigoIso = " xof ", Nome = " Franco CFA ", Simbolo = " CFA ", CasasDecimais = 0 });
        Assert.True(result.IsSuccess);
        Assert.Equal("XOF", gateway.Last?.CodigoIso);
        Assert.Equal("Franco CFA", gateway.Last?.Nome);
    }

    private sealed class FakeGateway : ICurrencyMasterDataGateway
    {
        public MoedaDto? Last { get; private set; }
        public Task<IReadOnlyList<MoedaDto>> ListAsync(string? search, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<MoedaDto>>(Array.Empty<MoedaDto>());
        public Task<int> SaveAsync(MoedaDto currency, CancellationToken cancellationToken = default) { Last = currency; return Task.FromResult(1); }
        public Task SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
