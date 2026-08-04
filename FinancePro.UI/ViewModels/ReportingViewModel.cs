using System.Collections.ObjectModel;
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
    public ReportDefinition? SelectedReport { get => _selectedReport; set => SetProperty(ref _selectedReport, value); }
    public DateTime? From { get => _from; set => SetProperty(ref _from, value); }
    public DateTime? To { get => _to; set => SetProperty(ref _to, value); }
    public bool Busy { get => _busy; set => SetProperty(ref _busy, value); }
    public string Message { get => _message; set => SetProperty(ref _message, value); }
    public int RowCount => _result?.RowCount ?? 0;
    public string ReportTitle => _result?.Title ?? "Pré-visualização";

    public ICommand GenerateCommand { get; }
    public ICommand ExportCommand { get; }

    public ReportingViewModel(IReportingService service, int companyId)
    {
        _service = service; _companyId = companyId;
        foreach (var item in service.GetCatalog()) Reports.Add(item);
        SelectedReport = Reports.FirstOrDefault();
        GenerateCommand = new AsyncRelayCommand(_ => GenerateAsync(), _ => SelectedReport is not null);
        ExportCommand = new AsyncRelayCommand(_ => ExportAsync(), _ => _result is not null && _result.RowCount > 0);
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
        var dialog = new SaveFileDialog { Filter = "Ficheiro CSV (*.csv)|*.csv", FileName = $"{_result.Title.Replace(' ', '-')}-{DateTime.Now:yyyyMMdd-HHmm}.csv" };
        if (dialog.ShowDialog() != true) return;
        await _service.ExportCsvAsync(_result, dialog.FileName);
        Message = "Relatório exportado com sucesso.";
    }

    private static string Format(object? value) => value switch { DateTime d => d.ToString("dd/MM/yyyy"), decimal n => n.ToString("N2"), _ => value?.ToString() ?? string.Empty };
}

public sealed record ReportPreviewRow(string Summary);
