using System.Globalization;
using System.Text;

namespace FinancePro.Platform.Reporting;

public sealed class ReportingService : IReportingService
{
    private readonly IReportDataProvider _provider;

    private static readonly IReadOnlyList<ReportDefinition> Catalog = new[]
    {
        new ReportDefinition("receivables", "Contas a Receber", "Títulos emitidos no período selecionado."),
        new ReportDefinition("payables", "Contas a Pagar", "Obrigações registadas no período selecionado."),
        new ReportDefinition("treasury", "Movimentos de Tesouraria", "Entradas, saídas e transferências do período."),
        new ReportDefinition("assets", "Inventário Patrimonial", "Bens patrimoniais e respetiva situação atual.")
    };

    public ReportingService(IReportDataProvider provider) => _provider = provider;

    public IReadOnlyList<ReportDefinition> GetCatalog() => Catalog;

    public Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CompanyId <= 0) throw new ArgumentException("Empresa inválida.", nameof(request));
        if (request.From.Date > request.To.Date) throw new ArgumentException("A data inicial não pode ser posterior à data final.", nameof(request));
        if (!Catalog.Any(x => x.Key.Equals(request.ReportKey, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Relatório inválido.", nameof(request));
        return _provider.GenerateAsync(request, cancellationToken);
    }

    public async Task ExportCsvAsync(ReportResult report, string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Caminho de exportação obrigatório.", nameof(filePath));

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(';', report.Columns.Select(c => Escape(c.Header))));
        foreach (var row in report.Rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.AppendLine(string.Join(';', report.Columns.Select(c => Escape(Format(row.Values.TryGetValue(c.Key, out var value) ? value : null)))));
        }
        await File.WriteAllTextAsync(filePath, "\uFEFF" + sb, new UTF8Encoding(false), cancellationToken);
    }

    private static string Format(object? value) => value switch
    {
        null => string.Empty,
        DateTime date => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        decimal number => number.ToString("N2", CultureInfo.GetCultureInfo("pt-PT")),
        double number => number.ToString("N2", CultureInfo.GetCultureInfo("pt-PT")),
        _ => Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty
    };

    private static string Escape(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
