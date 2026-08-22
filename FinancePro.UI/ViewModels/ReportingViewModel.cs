using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using FinancePro.Platform.Reporting;
using FinancePro.UI.Common;
using Microsoft.Win32;

namespace FinancePro.UI.ViewModels;

public sealed class ReportingViewModel : ViewModelBase
{
    private readonly IReportingService _service;
    private readonly int _companyId;
    private ReportDefinition? _selectedReport;
    private DateTime? _from = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime? _to = DateTime.Today;
    private ReportResult? _result;
    private bool _busy;
    private string _message = string.Empty;

    public ObservableCollection<ReportDefinition> Reports { get; } = new();
    public ObservableCollection<ReportPreviewRow> Rows { get; } = new();
    public ReportDefinition? SelectedReport { get => _selectedReport; set { if (SetProperty(ref _selectedReport, value)) OnPropertyChanged(nameof(SelectedReportDescription)); } }
    public string SelectedReportDescription => SelectedReport?.Description ?? string.Empty;
    public DateTime? From { get => _from; set => SetProperty(ref _from, value); }
    public DateTime? To { get => _to; set => SetProperty(ref _to, value); }
    public bool Busy { get => _busy; set => SetProperty(ref _busy, value); }
    public string Message { get => _message; set => SetProperty(ref _message, value); }
    public int RowCount => _result?.RowCount ?? 0;
    public string ReportTitle => _result?.Title ?? "Pré-visualização";

    public ICommand GenerateCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand PrintCommand { get; }
    public ICommand ExportExcelCommand { get; }
    public ICommand ExportPdfCommand { get; }

    public ReportingViewModel(IReportingService service, int companyId)
    {
        _service = service; _companyId = companyId;
        foreach (var item in service.GetCatalog()) Reports.Add(item);
        SelectedReport = Reports.FirstOrDefault();
        GenerateCommand = new AsyncRelayCommand(_ => GenerateAsync(), _ => SelectedReport is not null);
        ExportCommand = new AsyncRelayCommand(_ => ExportAsync(), _ => _result is not null && _result.RowCount > 0);
        PrintCommand = new AsyncRelayCommand(_ => PrintAsync(), _ => _result is not null && _result.RowCount > 0);
        ExportExcelCommand = new AsyncRelayCommand(_ => ExportExcelAsync(), _ => _result is not null && _result.RowCount > 0);
        ExportPdfCommand = new AsyncRelayCommand(_ => ExportPdfAsync(), _ => _result is not null && _result.RowCount > 0);
    }

    private async Task GenerateAsync()
    {
        if (SelectedReport is null || From is null || To is null) return;
        Busy = true; Message = string.Empty;
        try
        {
            _result = await _service.GenerateAsync(new ReportRequest(_companyId, SelectedReport.Key, From.Value, To.Value));
            Rows.Clear();
            foreach (var row in _result.Rows.Take(250))
                Rows.Add(new ReportPreviewRow(string.Join("  |  ", _result.Columns.Select(c => $"{c.Header}: {Format(row.Values.TryGetValue(c.Key, out var v) ? v : null)}"))));
            OnPropertyChanged(nameof(RowCount)); OnPropertyChanged(nameof(ReportTitle));
            Message = _result.RowCount > 250 ? "Pré-visualização limitada a 250 linhas. A exportação inclui todos os registos." : "Relatório gerado com sucesso.";
        }
        catch (Exception ex) { Message = ex.Message; }
        finally { Busy = false; }
    }

    private async Task ExportAsync()
    {
        if (_result is null) return;
        var dialog = new SaveFileDialog { Filter = "Ficheiro CSV (*.csv)|*.csv", FileName = $"{SafeFileName(_result.Title)}-{DateTime.Now:yyyyMMdd-HHmm}.csv" };
        if (dialog.ShowDialog() != true) return;
        await _service.ExportCsvAsync(_result, dialog.FileName);
        Message = "Relatório exportado com sucesso.";
    }

    private async Task ExportExcelAsync()
    {
        if (_result is null) return;
        var dialog = new SaveFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", FileName = $"{SafeFileName(_result.Title)}-{DateTime.Now:yyyyMMdd-HHmm}.xlsx" };
        if (dialog.ShowDialog() != true) return;
        await _service.ExportExcelAsync(_result, dialog.FileName);
        Message = "Relatório Excel exportado com sucesso.";
    }

    private async Task ExportPdfAsync()
    {
        if (_result is null) return;
        var dialog = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", FileName = $"{SafeFileName(_result.Title)}-{DateTime.Now:yyyyMMdd-HHmm}.pdf" };
        if (dialog.ShowDialog() != true) return;
        await _service.ExportPdfAsync(_result, dialog.FileName);
        Message = "Relatório PDF exportado com sucesso.";
    }

    private async Task PrintAsync()
    {
        if (_result is null) return;
        var filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"FinancePro-{SafeFileName(_result.Title)}-{DateTime.Now:yyyyMMddHHmmss}.html");
        await _service.ExportHtmlAsync(_result, filePath);
        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        Message = "Relatório aberto em modo de impressão. Utilize Imprimir ou Guardar como PDF no navegador.";
    }

    private static string SafeFileName(string value)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        return string.Concat(value.Select(c => invalid.Contains(c) || char.IsWhiteSpace(c) ? '-' : c)).Trim('-');
    }

    private static string Format(object? value) => value switch { DateTime d => d.ToString("dd/MM/yyyy"), decimal n => n.ToString("N2"), _ => value?.ToString() ?? string.Empty };
}

public sealed record ReportPreviewRow(string Summary);

