using FinancePro.Platform.Reporting;
using Xunit;

namespace FinancePro.Application.Tests.Platform;

public sealed class ReportingServiceTests
{
    [Fact]
    public async Task Generate_rejects_invalid_period()
    {
        var service = new ReportingService(new FakeProvider());
        await Assert.ThrowsAsync<ArgumentException>(() => service.GenerateAsync(new ReportRequest(1, "receivables", DateTime.Today, DateTime.Today.AddDays(-1))));
    }

    [Fact]
    public async Task Generate_returns_provider_result()
    {
        var service = new ReportingService(new FakeProvider());
        var result = await service.GenerateAsync(new ReportRequest(1, "receivables", DateTime.Today, DateTime.Today));
        Assert.Equal(1, result.RowCount);
    }

    private sealed class FakeProvider : IReportDataProvider
    {
        public Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ReportResult("Teste", new[] { new ReportColumn("A", "A") }, new[] { new ReportRow(new Dictionary<string, object?> { ["A"] = 1 }) }));
    }
}
