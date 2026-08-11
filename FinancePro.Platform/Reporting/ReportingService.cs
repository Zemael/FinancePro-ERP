using System.Globalization;
using System.Net;
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
        new ReportDefinition("assets", "Inventário Patrimonial", "Bens patrimoniais e respetiva situação atual."),
        new ReportDefinition("fiscal", "Conformidade Fiscal", "Obrigações fiscais, vencimentos e respetivo estado."),
        new ReportDefinition("accounting-journal", "Diário Contabilístico", "Lançamentos contabilísticos e respetivas partidas no período."),
        new ReportDefinition("accounting-ledger", "Razão Geral", "Movimentos e saldo acumulado, agrupados por conta contabilística."),
        new ReportDefinition("accounting-trial-balance", "Balancete", "Saldos iniciais, movimentos e saldos finais das contas."),
        new ReportDefinition("accounting-income-statement", "DRE", "Demonstração do Resultado com comparativo do período imediatamente anterior."),
        new ReportDefinition("accounting-balance-sheet", "Balanço Patrimonial", "Posição patrimonial na data final e comparativo no início do período."),
        new ReportDefinition("accounting-cash-flow", "Fluxo de Caixa", "Fluxos operacionais, de investimento e financiamento no período.")
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

    public async Task ExportHtmlAsync(ReportResult report, string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Caminho de exportação obrigatório.", nameof(filePath));

        var sb = new StringBuilder();
        sb.Append("<!doctype html><html lang=\"pt\"><head><meta charset=\"utf-8\"><title>")
          .Append(Html(report.Title))
          .Append("</title><style>")
          .Append("body{font-family:Segoe UI,Arial,sans-serif;margin:28px;color:#233247}h1{color:#0E4C5E;font-size:22px;margin:0 0 6px}p.meta{color:#667085;margin:0 0 18px}table{border-collapse:collapse;width:100%;font-size:12px}th{background:#F1F5F9;text-align:left;color:#0E4C5E}th,td{border:1px solid #D1DCE8;padding:7px 9px;vertical-align:top}tr:nth-child(even){background:#FAFCFD}@media print{body{margin:10mm}button{display:none}thead{display:table-header-group}}")
          .Append("</style></head><body><button onclick=\"window.print()\" style=\"float:right;padding:8px 14px\">Imprimir</button><h1>")
          .Append(Html(report.Title)).Append("</h1><p class=\"meta\">Gerado em ")
          .Append(DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)).Append(" · ")
          .Append(report.RowCount).Append(" registo(s)</p><table><thead><tr>");

        foreach (var column in report.Columns) sb.Append("<th>").Append(Html(column.Header)).Append("</th>");
        sb.Append("</tr></thead><tbody>");
        foreach (var row in report.Rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.Append("<tr>");
            foreach (var column in report.Columns)
            {
                row.Values.TryGetValue(column.Key, out var value);
                sb.Append("<td>").Append(Html(Format(value))).Append("</td>");
            }
            sb.Append("</tr>");
        }
        sb.Append("</tbody></table></body></html>");
        await File.WriteAllTextAsync(filePath, sb.ToString(), new UTF8Encoding(false), cancellationToken);
    }

    private static string Format(object? value) => value switch
    {
        null => string.Empty,
        DateTime date => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        decimal number => number.ToString("N2", CultureInfo.GetCultureInfo("pt-PT")),
        double number => number.ToString("N2", CultureInfo.GetCultureInfo("pt-PT")),
        bool b => b ? "Sim" : "Não",
        _ => Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty
    };

    private static string Escape(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
    private static string Html(string value) => WebUtility.HtmlEncode(value);
}
