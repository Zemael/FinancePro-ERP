using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace FinancePro.Platform.Reporting;

internal static class ProfessionalReportExporter
{
    public static async Task ExportExcelAsync(ReportResult report, string filePath, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Caminho de exportação obrigatório.", nameof(filePath));
        await Task.Run(() => WriteXlsx(report, filePath, ct), ct);
    }

    public static async Task ExportPdfAsync(ReportResult report, string filePath, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Caminho de exportação obrigatório.", nameof(filePath));
        await Task.Run(() => WritePdf(report, filePath, ct), ct);
    }

    private static void WriteXlsx(ReportResult report, string path, CancellationToken ct)
    {
        using var fs = File.Create(path);
        using var zip = new ZipArchive(fs, ZipArchiveMode.Create);
        Add(zip, "[Content_Types].xml", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/><Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/></Types>""");
        Add(zip, "_rels/.rels", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>""");
        Add(zip, "xl/workbook.xml", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Relatório" sheetId="1" r:id="rId1"/></sheets></workbook>""");
        Add(zip, "xl/_rels/workbook.xml.rels", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/><Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/></Relationships>""");
        Add(zip, "xl/styles.xml", StylesXml);

        var e = zip.CreateEntry("xl/worksheets/sheet1.xml", CompressionLevel.Optimal);
        using var w = new StreamWriter(e.Open(), new UTF8Encoding(false));
        w.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetViews><sheetView workbookViewId=\"0\"><pane ySplit=\"6\" topLeftCell=\"A7\" activePane=\"bottomLeft\" state=\"frozen\"/></sheetView></sheetViews><sheetData>");
        Row(w, 1, new[] { report.CompanyName.Length > 0 ? report.CompanyName : "FinancePro ERP" }, 2);
        Row(w, 2, new[] { string.IsNullOrWhiteSpace(report.CompanyTaxNumber) ? "" : $"NIF: {report.CompanyTaxNumber}" }, 3);
        Row(w, 3, new[] { report.Title }, 2);
        Row(w, 4, new[] { Period(report) }, 3);
        Row(w, 5, new[] { $"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}" }, 3);
        Row(w, 6, report.Columns.Select(x => x.Header), 1);
        var r = 7;
        foreach (var row in report.Rows)
        {
            ct.ThrowIfCancellationRequested();
            Row(w, r++, report.Columns.Select(c => Format(row.Values.TryGetValue(c.Key, out var v) ? v : null)), 0);
        }
        w.Write("</sheetData><autoFilter ref=\"A6:" + Col(Math.Max(1, report.Columns.Count)) + Math.Max(6, r - 1) + "\"/><pageSetup orientation=\"" + (report.Columns.Count > 6 ? "landscape" : "portrait") + "\" fitToWidth=\"1\" fitToHeight=\"0\"/><pageMargins left=\"0.3\" right=\"0.3\" top=\"0.5\" bottom=\"0.5\" header=\"0.2\" footer=\"0.2\"/></worksheet>");
    }

    private static void Row(StreamWriter w, int row, IEnumerable<string> values, int style)
    {
        w.Write($"<row r=\"{row}\">"); var c = 1;
        foreach (var value in values)
        {
            var cell = Col(c++) + row;
            w.Write($"<c r=\"{cell}\" t=\"inlineStr\" s=\"{style}\"><is><t xml:space=\"preserve\">{Xml(value)}</t></is></c>");
        }
        w.Write("</row>");
    }

    private static void WritePdf(ReportResult report, string path, CancellationToken ct)
    {
        var landscape = report.Columns.Count > 6;
        var width = landscape ? 842d : 595d; var height = landscape ? 595d : 842d;
        var linesPerPage = landscape ? 29 : 44;
        var pages = report.Rows.Chunk(linesPerPage).ToArray();
        if (pages.Length == 0) pages = new[] { Array.Empty<ReportRow>() };
        var objects = new List<string>();
        objects.Add("<< /Type /Catalog /Pages 2 0 R >>");
        var pageIds = Enumerable.Range(0, pages.Length).Select(i => 4 + i * 2).ToArray();
        objects.Add($"<< /Type /Pages /Kids [{string.Join(' ', pageIds.Select(x => $"{x} 0 R"))}] /Count {pages.Length} >>");
        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
        for (var i = 0; i < pages.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            var contentId = pageIds[i] + 1;
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {width:0} {height:0}] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentId} 0 R >>");
            var content = PdfPage(report, pages[i], i + 1, pages.Length, width, height);
            objects.Add($"<< /Length {Encoding.Latin1.GetByteCount(content)} >>\nstream\n{content}\nendstream");
        }
        using var fs = File.Create(path); using var bw = new BinaryWriter(fs, Encoding.Latin1);
        bw.Write(Encoding.Latin1.GetBytes("%PDF-1.4\n")); var offsets = new List<long> { 0 };
        for (var i = 0; i < objects.Count; i++) { offsets.Add(fs.Position); bw.Write(Encoding.Latin1.GetBytes($"{i + 1} 0 obj\n{objects[i]}\nendobj\n")); }
        var xref = fs.Position; bw.Write(Encoding.Latin1.GetBytes($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n"));
        for (var i = 1; i < offsets.Count; i++) bw.Write(Encoding.Latin1.GetBytes($"{offsets[i]:0000000000} 00000 n \n"));
        bw.Write(Encoding.Latin1.GetBytes($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF"));
    }

    private static string PdfPage(ReportResult report, ReportRow[] rows, int page, int total, double width, double height)
    {
        var sb = new StringBuilder(); double y = height - 35;
        void Text(double x, double yy, int size, string text) => sb.Append($"BT /F1 {size} Tf {x:0.##} {yy:0.##} Td ({Pdf(text)}) Tj ET\n");
        Text(32, y, 13, report.CompanyName.Length > 0 ? report.CompanyName : "FinancePro ERP"); y -= 16;
        if (!string.IsNullOrWhiteSpace(report.CompanyTaxNumber)) { Text(32, y, 8, $"NIF: {report.CompanyTaxNumber}"); y -= 13; }
        Text(32, y, 12, report.Title); y -= 15; Text(32, y, 8, Period(report)); y -= 18;
        var usable = width - 64; var colW = usable / Math.Max(1, report.Columns.Count);
        for (var i = 0; i < report.Columns.Count; i++) Text(32 + i * colW, y, 7, Cut(report.Columns[i].Header, (int)Math.Max(6, colW / 4.2))); y -= 13;
        foreach (var row in rows)
        {
            for (var i = 0; i < report.Columns.Count; i++) { row.Values.TryGetValue(report.Columns[i].Key, out var v); Text(32 + i * colW, y, 6, Cut(Format(v), (int)Math.Max(6, colW / 3.6))); }
            y -= 12;
        }
        Text(32, 20, 7, $"FinancePro ERP · Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}"); Text(width - 100, 20, 7, $"Página {page}/{total}");
        return sb.ToString();
    }

    private static string Period(ReportResult r) => r.PeriodFrom.HasValue && r.PeriodTo.HasValue ? $"Período: {r.PeriodFrom:dd/MM/yyyy} a {r.PeriodTo:dd/MM/yyyy}" : string.Empty;
    private static string Format(object? v) => v switch { null => "", DateTime d => d.ToString("dd/MM/yyyy"), decimal n => n.ToString("N2", CultureInfo.GetCultureInfo("pt-PT")), double n => n.ToString("N2", CultureInfo.GetCultureInfo("pt-PT")), bool b => b ? "Sim" : "Não", _ => Convert.ToString(v, CultureInfo.CurrentCulture) ?? "" };
    private static string Cut(string s, int n) => s.Length <= n ? s : s[..Math.Max(1, n - 1)] + "…";
    private static string Xml(string s) => System.Security.SecurityElement.Escape(s) ?? "";
    private static string Pdf(string s) => s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)").Replace("\r", " ").Replace("\n", " ").Select(ch => ch <= 255 ? ch : '?').Aggregate(new StringBuilder(), (b,ch)=>b.Append(ch)).ToString();
    private static string Col(int n) { var s = ""; while (n > 0) { n--; s = (char)('A' + n % 26) + s; n /= 26; } return s; }
    private static void Add(ZipArchive z, string name, string text) { var e = z.CreateEntry(name, CompressionLevel.Optimal); using var w = new StreamWriter(e.Open(), new UTF8Encoding(false)); w.Write(text); }

    private const string StylesXml = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><fonts count="3"><font><sz val="10"/><name val="Calibri"/></font><font><b/><color rgb="FFFFFFFF"/><sz val="10"/><name val="Calibri"/></font><font><b/><color rgb="FF0E4C5E"/><sz val="14"/><name val="Calibri"/></font></fonts><fills count="3"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill><fill><patternFill patternType="solid"><fgColor rgb="FF0E4C5E"/><bgColor indexed="64"/></patternFill></fill></fills><borders count="2"><border/><border><left style="thin"><color rgb="FFD1DCE8"/></left><right style="thin"><color rgb="FFD1DCE8"/></right><top style="thin"><color rgb="FFD1DCE8"/></top><bottom style="thin"><color rgb="FFD1DCE8"/></bottom></border></borders><cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs><cellXfs count="4"><xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0"/><xf numFmtId="0" fontId="1" fillId="2" borderId="1" xfId="0" applyFill="1" applyFont="1"/><xf numFmtId="0" fontId="2" fillId="0" borderId="0" xfId="0" applyFont="1"/><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/></cellXfs></styleSheet>""";
}
