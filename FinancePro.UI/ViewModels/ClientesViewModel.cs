using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Application.Customers;
using FinancePro.Core.DTOs;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class ClientesViewModel : ViewModelBase
{
    private readonly CustomerApplicationService _service;
    private readonly int _empresaId;
    private ClienteDto? _selecionado;
    private string _pesquisa = string.Empty;
    private string _nome = string.Empty;
    private string? _nif;
    private string? _telefone;
    private string? _email;
    private string? _morada;
    private string? _mensagem;
    private bool _ocupado;

    public ClientesViewModel(CustomerApplicationService service, int empresaId)
    {
        _service=service; _empresaId=empresaId;
        AtualizarCommand=new AsyncRelayCommand(_ => LoadAsync());
        GuardarCommand=new AsyncRelayCommand(_ => SaveAsync());
        NovoCommand=new RelayCommand(_ => Clear());
        EditarCommand=new RelayCommand(LoadSelected);
        AlternarAtivoCommand=new AsyncRelayCommand(ToggleAsync);
        _ = LoadAsync();
    }

    public ObservableCollection<ClienteDto> Itens { get; } = new();

    public int TotalClientes => Itens.Count;
    public int ClientesAtivos => Itens.Count(x => x.Ativo);
    public int ClientesInativos => Itens.Count(x => !x.Ativo);
    public int ClientesComEmail => Itens.Count(x => !string.IsNullOrWhiteSpace(x.Email));
    public string UltimaAtualizacao { get; private set; } = "--";
    public ClienteDto? Selecionado { get => _selecionado; set => SetProperty(ref _selecionado, value); }
    public string Pesquisa { get => _pesquisa; set => SetProperty(ref _pesquisa, value); }
    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string? NIF { get => _nif; set => SetProperty(ref _nif, value); }
    public string? Telefone { get => _telefone; set => SetProperty(ref _telefone, value); }
    public string? Email { get => _email; set => SetProperty(ref _email, value); }
    public string? Morada { get => _morada; set => SetProperty(ref _morada, value); }
    public string? Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public bool Ocupado { get => _ocupado; set => SetProperty(ref _ocupado, value); }
    public ICommand AtualizarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand AlternarAtivoCommand { get; }

    private async Task LoadAsync()
    {
        Ocupado=true; Mensagem=null;
        var result=await _service.ListAsync(_empresaId, Pesquisa);
        Itens.Clear();
        if (result.IsSuccess && result.Value is not null) foreach (var item in result.Value) Itens.Add(item);
        else Mensagem=result.Message ?? string.Join(Environment.NewLine, result.Errors);
        UltimaAtualizacao = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        OnPropertyChanged(nameof(TotalClientes));
        OnPropertyChanged(nameof(ClientesAtivos));
        OnPropertyChanged(nameof(ClientesInativos));
        OnPropertyChanged(nameof(ClientesComEmail));
        OnPropertyChanged(nameof(UltimaAtualizacao));
        Ocupado=false;
    }

    private async Task SaveAsync()
    {
        Ocupado=true;
        var result=await _service.SaveAsync(new CustomerSaveRequest(Selecionado?.Id ?? 0, _empresaId, Nome, NIF, Telefone, Email, Morada, Selecionado?.Ativo ?? true));
        Mensagem=result.IsSuccess ? result.Message : result.Message ?? string.Join(Environment.NewLine, result.Errors);
        if (result.IsSuccess) { Clear(); await LoadAsync(); }
        Ocupado=false;
    }

    private async Task ToggleAsync(object? parameter)
    {
        var item=parameter as ClienteDto ?? Selecionado; if (item is null) return;
        var result=await _service.SetActiveAsync(item.Id, !item.Ativo);
        Mensagem=result.IsSuccess ? result.Message : result.Message ?? string.Join(Environment.NewLine, result.Errors);
        if (result.IsSuccess) await LoadAsync();
    }

    private void LoadSelected(object? parameter)
    {
        if (parameter is ClienteDto item) Selecionado = item;
        if (Selecionado is null) return;
        Nome=Selecionado.Nome; NIF=Selecionado.NIF; Telefone=Selecionado.Telefone; Email=Selecionado.Email; Morada=Selecionado.Morada;
    }

    private void Clear()
    {
        Selecionado=null; Nome=string.Empty; NIF=null; Telefone=null; Email=null; Morada=null; Mensagem=null;
    }
}
