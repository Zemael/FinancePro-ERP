using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Platform.Documents;
using FinancePro.UI.Common;
using Microsoft.Win32;

namespace FinancePro.UI.ViewModels;

public sealed class DocumentsViewModel : ViewModelBase
{
    private readonly IDocumentService _service;
    private DocumentTemplate? _selectedTemplate;
    private GeneratedDocument? _generated;
    private string _preview = "Selecione um modelo e preencha os campos para gerar o documento.";
    private string _message = string.Empty;

    public ObservableCollection<DocumentTemplate> Templates { get; } = new();
    public ObservableCollection<DocumentFieldEditor> Fields { get; } = new();

    public DocumentTemplate? SelectedTemplate
    {
        get => _selectedTemplate;
        set
        {
            if (!SetProperty(ref _selectedTemplate, value)) return;
            LoadFields();
        }
    }

    public string Preview { get => _preview; set => SetProperty(ref _preview, value); }
    public string Message { get => _message; set => SetProperty(ref _message, value); }
    public string TemplateDescription => SelectedTemplate?.Description ?? "Escolha um modelo institucional.";
    public int FieldCount => Fields.Count;

    public ICommand GenerateCommand { get; }
    public ICommand ExportTextCommand { get; }
    public ICommand ExportHtmlCommand { get; }
    public ICommand ClearCommand { get; }

    public DocumentsViewModel(IDocumentService service)
    {
        _service = service;
        foreach (var template in service.GetTemplates()) Templates.Add(template);
        GenerateCommand = new RelayCommand(_ => Generate(), _ => SelectedTemplate is not null);
        ExportTextCommand = new AsyncRelayCommand(_ => ExportTextAsync(), _ => _generated is not null);
        ExportHtmlCommand = new AsyncRelayCommand(_ => ExportHtmlAsync(), _ => _generated is not null);
        ClearCommand = new RelayCommand(_ => Clear(), _ => Fields.Count > 0);
        SelectedTemplate = Templates.FirstOrDefault();
    }

    private void LoadFields()
    {
        Fields.Clear();
        _generated = null;
        Preview = "Preencha os campos e clique em Gerar documento.";
        Message = string.Empty;
        if (SelectedTemplate is not null)
            foreach (var field in _service.GetFields(SelectedTemplate.Key)) Fields.Add(new DocumentFieldEditor(field.Key, field.Label, field.Value));
        OnPropertyChanged(nameof(TemplateDescription));
        OnPropertyChanged(nameof(FieldCount));
    }

    private void Generate()
    {
        if (SelectedTemplate is null) return;
        var values = Fields.ToDictionary(x => x.Key, x => x.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        _generated = _service.Generate(new DocumentRequest(SelectedTemplate.Key, values));
        Preview = _generated.Content;
        Message = "Documento gerado com sucesso.";
    }

    private async Task ExportTextAsync()
    {
        if (_generated is null) return;
        var dialog = new SaveFileDialog { Filter = "Documento de texto (*.txt)|*.txt", FileName = FileName("txt") };
        if (dialog.ShowDialog() != true) return;
        await _service.ExportTextAsync(_generated, dialog.FileName);
        Message = "Documento de texto exportado com sucesso.";
    }

    private async Task ExportHtmlAsync()
    {
        if (_generated is null) return;
        var dialog = new SaveFileDialog { Filter = "Documento HTML (*.html)|*.html", FileName = FileName("html") };
        if (dialog.ShowDialog() != true) return;
        await _service.ExportHtmlAsync(_generated, dialog.FileName);
        Message = "Documento HTML exportado com sucesso.";
    }

    private string FileName(string extension) => $"{(_generated?.Title ?? "documento").Replace(' ', '-')}-{DateTime.Now:yyyyMMdd-HHmm}.{extension}";

    private void Clear()
    {
        foreach (var field in Fields) field.Value = string.Empty;
        _generated = null;
        Preview = "Campos limpos. Preencha os dados para gerar um novo documento.";
        Message = string.Empty;
    }
}

public sealed class DocumentFieldEditor : ViewModelBase
{
    private string _value;
    public string Key { get; }
    public string Label { get; }
    public string Value { get => _value; set => SetProperty(ref _value, value); }
    public bool IsLongText => Key is "Corpo" or "Analise" or "Enquadramento" or "Conclusao" or "Itens" or "Observacoes";

    public DocumentFieldEditor(string key, string label, string value)
    {
        Key = key; Label = label; _value = value;
    }
}
