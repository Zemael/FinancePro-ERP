namespace FinancePro.Platform.Reporting;

public sealed record ReportDefinition(string Key, string Name, string Description);

public sealed record ReportRequest(int CompanyId, string ReportKey, DateTime From, DateTime To);

public sealed record ReportColumn(string Key, string Header);

public sealed record ReportRow(IReadOnlyDictionary<string, object?> Values);

public sealed record ReportResult(
    string Title,
    IReadOnlyList<ReportColumn> Columns,
    IReadOnlyList<ReportRow> Rows)
{
    public int RowCount => Rows.Count;
}
