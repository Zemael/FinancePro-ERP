using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Application.MasterData.Partners;
using FinancePro.Core.DTOs;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class ParceirosViewModel : ViewModelBase
{
    private readonly BusinessPartnerApplicationService _service;
    private readonly int _empresaId;
    private int _idEdicao;
    private string _tipo = "Cliente";
    private string _filtroTipo = string.Empty;
    private string _pesquisa = string.Empty;
    private string _nome = string.Empty;
    private string _nif = string.Empty;
    private string _telefone = string.Empty;
    private string _email = string.Empty;
    private string _morada = string.Empty;
    private string _mensagem = string.Empty;
    private bool _aCarregar;
    private bool _aGuardar;
    private BusinessPartnerDto? _selecionado;

    public ObservableCollection<BusinessPartnerDto> Parceiros { get; } = new();
    public ObservableCollection<string> Tipos { get; } = new() { "Cliente", "Fornecedor" };
    public ObservableCollection<string> FiltrosTipo { get; } = new() { string.Empty, "Cliente", "Fornecedor" };

    public string Tipo { get => _tipo; set => SetProperty(ref _tipo, value); }
    public string FiltroTipo { get => _filtroTipo; set => SetProperty(ref _filtroTipo, value); }
    public string Pesquisa { get => _pesquisa; set => SetProperty(ref _pesquisa, value); }
    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string NIF { get => _nif; set => SetProperty(ref _nif, value); }
    public string Telefone { get => _telefone; set => SetProperty(ref _telefone, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Morada { get => _morada; set => SetProperty(ref _morada, value); }
    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }
    public BusinessPartnerDto? Selecionado { get => _selecionado; set => SetProperty(ref _selecionado, value); }

    public ICommand AtualizarCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand AlternarAtivoCommand { get; }

    public ParceirosViewModel(BusinessPartnerApplicationService service, int empresaId)
    {
        _service = service; _empresaId = empresaId;
        AtualizarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        NovoCommand = new RelayCommand(_ => Limpar());
        EditarCommand = new AsyncRelayCommand(_ => EditarAsync(), _ => Selecionado is not null);
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => !AGuardar);
        AlternarAtivoCommand = new AsyncRelayCommand(AlternarAtivoAsync);
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        ACarregar = true; Mensagem = string.Empty;
        try
        {
            var result = await _service.ListAsync(_empresaId, FiltroTipo, Pesquisa);
            if (result.IsFailure) { Mensagem = Erro(result.Errors, result.Message); return; }
            Parceiros.Clear();
            foreach (var item in result.Value ?? Array.Empty<BusinessPartnerDto>()) Parceiros.Add(item);
        }
        finally { ACarregar = false; }
    }

    private async Task EditarAsync()
    {
        if (Selecionado is null) return;
        var result = await _service.GetAsync(_empresaId, Selecionado.Tipo, Selecionado.Id);
        if (result.IsFailure || result.Value is null) { Mensagem = Erro(result.Errors, result.Message); return; }
        var item = result.Value; _idEdicao=item.Id; Tipo=item.Tipo; Nome=item.Nome; NIF=item.NIF ?? string.Empty; Telefone=item.Telefone ?? string.Empty; Email=item.Email ?? string.Empty; Morada=item.Morada ?? string.Empty;
        Mensagem = "Parceiro carregado para edição.";
    }

    private async Task GuardarAsync()
    {
        AGuardar = true; Mensagem = string.Empty;
        try
        {
            var result = await _service.SaveAsync(new BusinessPartnerSaveRequest(_idEdicao, _empresaId, Tipo, Nome, NIF, Telefone, Email, Morada));
            if (result.IsFailure) { Mensagem = Erro(result.Errors, result.Message); return; }
            Limpar(); Mensagem = result.Message ?? "Parceiro guardado com sucesso."; await CarregarAsync();
        }
        finally { AGuardar = false; }
    }

    private async Task AlternarAtivoAsync(object? parameter)
    {
        if (parameter is not BusinessPartnerDto item) return;
        var result = await _service.SetActiveAsync(_empresaId, item.Tipo, item.Id, !item.Ativo);
        Mensagem = result.IsSuccess ? result.Message ?? "Estado atualizado." : Erro(result.Errors, result.Message);
        if (result.IsSuccess) await CarregarAsync();
    }

    private void Limpar()
    {
        _idEdicao = 0; Tipo = "Cliente"; Nome = NIF = Telefone = Email = Morada = string.Empty; Selecionado = null; Mensagem = string.Empty;
    }

    private static string Erro(IReadOnlyCollection<string> errors, string? message) => errors.Count > 0 ? string.Join(Environment.NewLine, errors) : message ?? "Ocorreu um erro.";
}
