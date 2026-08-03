using FinancePro.Application.MasterData.Partners;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.MasterData;

public sealed class BusinessPartnerApplicationServiceTests
{
    [Fact]
    public async Task SaveAsync_ShouldRejectShortName()
    {
        var service = new BusinessPartnerApplicationService(new FakeGateway());
        var result = await service.SaveAsync(new BusinessPartnerSaveRequest(0, 1, "Cliente", "A", null, null, null, null));
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SaveAsync_ShouldNormalizeAndSaveSupplier()
    {
        var gateway = new FakeGateway();
        var service = new BusinessPartnerApplicationService(gateway);
        var result = await service.SaveAsync(new BusinessPartnerSaveRequest(0, 1, "fornecedor", "  Empresa X  ", " 123 ", null, " INFO@X.COM ", null));
        Assert.True(result.IsSuccess);
        Assert.Equal("Fornecedor", gateway.LastRequest?.Tipo);
        Assert.Equal("Empresa X", gateway.LastRequest?.Nome);
        Assert.Equal("info@x.com", gateway.LastRequest?.Email);
    }

    [Fact]
    public async Task ListAsync_ShouldRejectInvalidCompany()
    {
        var service = new BusinessPartnerApplicationService(new FakeGateway());
        var result = await service.ListAsync(0, null, null);
        Assert.True(result.IsFailure);
    }

    private sealed class FakeGateway : IBusinessPartnerGateway
    {
        public BusinessPartnerSaveRequest? LastRequest { get; private set; }
        public Task<IReadOnlyList<BusinessPartnerDto>> ListAsync(int empresaId, string? tipo, string? pesquisa, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<BusinessPartnerDto>>(Array.Empty<BusinessPartnerDto>());
        public Task<BusinessPartnerDto?> GetAsync(int empresaId, string tipo, int id, CancellationToken cancellationToken = default) => Task.FromResult<BusinessPartnerDto?>(null);
        public Task<int> SaveAsync(BusinessPartnerSaveRequest request, CancellationToken cancellationToken = default) { LastRequest = request; return Task.FromResult(10); }
        public Task SetActiveAsync(int empresaId, string tipo, int id, bool ativo, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
