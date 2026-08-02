using FinancePro.Application.MasterData.Companies;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.MasterData;

public sealed class CompanyApplicationServiceTests
{
    [Fact]
    public async Task SaveAsync_ShouldFail_WhenNameIsTooShort()
    {
        var service = new CompanyApplicationService(new FakeGateway());
        var result = await service.SaveAsync(new CompanySaveRequest(0, "AB", null, null, null, null, "FCFA"));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SaveAsync_ShouldNormalizeCompanyData()
    {
        var gateway = new FakeGateway();
        var service = new CompanyApplicationService(gateway);

        var result = await service.SaveAsync(new CompanySaveRequest(
            0,
            "  FinancePro SARL  ",
            " 123 ",
            " Bissau ",
            " 955000000 ",
            " ADMIN@FINANCEPRO.GW ",
            " fcfa "));

        Assert.True(result.IsSuccess);
        Assert.NotNull(gateway.LastRequest);
        Assert.Equal("FinancePro SARL", gateway.LastRequest!.Name);
        Assert.Equal("admin@financepro.gw", gateway.LastRequest.Email);
        Assert.Equal("FCFA", gateway.LastRequest.Currency);
    }

    [Fact]
    public async Task SetActiveAsync_ShouldFail_WhenIdIsInvalid()
    {
        var service = new CompanyApplicationService(new FakeGateway());
        var result = await service.SetActiveAsync(0, true);

        Assert.True(result.IsFailure);
    }

    private sealed class FakeGateway : ICompanyGateway
    {
        public CompanySaveRequest? LastRequest { get; private set; }

        public Task<IReadOnlyList<EmpresaListItemDto>> ListAsync(string? search, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EmpresaListItemDto>>(Array.Empty<EmpresaListItemDto>());

        public Task<EmpresaDto?> GetAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult<EmpresaDto?>(new EmpresaDto { Id = id, Nome = "Empresa", Moeda = "FCFA" });

        public Task<int> SaveAsync(CompanySaveRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(1);
        }

        public Task SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
