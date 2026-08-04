namespace FinancePro.Platform.Reporting;

public interface IReportDataProvider
{
    Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken cancellationToken = default);
}

public interface IReportingService
{
    IReadOnlyList<ReportDefinition> GetCatalog();
    Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken cancellationToken = default);
    Task ExportCsvAsync(ReportResult report, string filePath, CancellationToken cancellationToken = default);
}
