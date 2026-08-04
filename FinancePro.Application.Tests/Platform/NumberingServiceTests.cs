using FinancePro.Application.Platform;
using Xunit;

namespace FinancePro.Application.Tests.Platform;

public sealed class NumberingServiceTests
{
    [Fact]
    public async Task Normalizes_module_and_prefix()
    {
        var gateway = new FakeGateway();
        var service = new NumberingService(gateway);
        var result = await service.GetNextAsync(new DocumentNumberRequest(1, " rec ", " rec ", new DateTime(2026, 8, 3)));
        Assert.Equal("REC-2026-000001", result);
        Assert.Equal("REC", gateway.Request!.Modulo);
    }

    [Fact]
    public async Task Rejects_invalid_company()
    {
        var service = new NumberingService(new FakeGateway());
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetNextAsync(new DocumentNumberRequest(0, "REC", "REC", DateTime.Today)));
    }

    private sealed class FakeGateway : INumberingGateway
    {
        public DocumentNumberRequest? Request { get; private set; }
        public Task<string> GetNextAsync(DocumentNumberRequest request, CancellationToken cancellationToken = default)
        {
            Request = request;
            return Task.FromResult($"{request.Prefixo}-{request.DataDocumento.Year}-000001");
        }
    }
}
