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

    [Fact]
    public void Catalog_contains_financial_statements()
    {
        var service = new ReportingService(new FakeProvider());
        var keys = service.GetCatalog().Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("accounting-journal", keys);
        Assert.Contains("accounting-ledger", keys);
        Assert.Contains("accounting-trial-balance", keys);
        Assert.Contains("accounting-income-statement", keys);
        Assert.Contains("accounting-balance-sheet", keys);
        Assert.Contains("accounting-cash-flow", keys);
    }

    [Fact]
    public async Task ExportHtml_creates_printable_report()
    {
        var service = new ReportingService(new FakeProvider());
        var report = await service.GenerateAsync(new ReportRequest(1, "receivables", DateTime.Today, DateTime.Today));
        var file = Path.Combine(Path.GetTempPath(), $"financepro-report-{Guid.NewGuid():N}.html");
        try
        {
            await service.ExportHtmlAsync(report, file);
            var html = await File.ReadAllTextAsync(file);
            Assert.Contains("<table>", html);
            Assert.Contains("window.print()", html);
            Assert.Contains("Teste", html);
        }
        finally
        {
            if (File.Exists(file)) File.Delete(file);
        }
    }

    private sealed class FakeProvider : IReportDataProvider
    {
        public Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ReportResult("Teste", new[] { new ReportColumn("A", "A") }, new[] { new ReportRow(new Dictionary<string, object?> { ["A"] = 1 }) }));
    }
}
