using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Application.MasterData.Companies;
using FinancePro.Core.DTOs;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class EmpresasViewModel : ViewModelBase
{
    private readonly CompanyApplicationService _service;
    private string _pesquisa = string.Empty;
    private EmpresaListItemDto? _selecionada;
    private int _idEdicao;
    private string _nome = string.Empty;
    private string _nif = string.Empty;
    private string _telefone = string.Empty;
    private string _email = string.Empty;
    private string _morada = string.Empty;
    private string _moeda = "FCFA";
    private string _mensagem = string.Empty;
    private bool _aGuardar;
    private bool _aCarregar;

    public ObservableCollection<EmpresaListItemDto> Empresas { get; } = new();
    public string Pesquisa { get => _pesquisa; set => SetProperty(ref _pesquisa, value); }
    public EmpresaListItemDto? Selecionada { get => _selecionada; set => SetProperty(ref _selecionada, value); }
    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string NIF { get => _nif; set => SetProperty(ref _nif, value); }
    public string Telefone { get => _telefone; set => SetProperty(ref _telefone, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Morada { get => _morada; set => SetProperty(ref _morada, value); }
    public string Moeda { get => _moeda; set => SetProperty(ref _moeda, value); }
    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }
    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }

    public ICommand PesquisarCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand AlternarAtivoCommand { get; }

    public EmpresasViewModel(CompanyApplicationService service)
    {
        _service = service;
        PesquisarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        NovoCommand = new RelayCommand(_ => LimparFormulario());
        EditarCommand = new AsyncRelayCommand(_ => EditarAsync(), _ => Selecionada is not null);
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => !AGuardar);
        AlternarAtivoCommand = new AsyncRelayCommand(AlternarAtivoAsync);
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        ACarregar = true;
        Mensagem = string.Empty;
        try
        {
            var result = await _service.ListAsync(Pesquisa);
            if (result.IsFailure)
            {
                Mensagem = ObterErro(result.Errors, result.Message);
                return;
            }

            Empresas.Clear();
            foreach (var item in result.Value ?? Array.Empty<EmpresaListItemDto>())
                Empresas.Add(item);
        }
        finally
        {
            ACarregar = false;
        }
    }

    private async Task EditarAsync()
    {
        if (Selecionada is null) return;

        var result = await _service.GetAsync(Selecionada.Id);
        if (result.IsFailure || result.Value is null)
        {
            Mensagem = ObterErro(result.Errors, result.Message);
            return;
        }

        var dto = result.Value;
        _idEdicao = dto.Id;
        Nome = dto.Nome;
        NIF = dto.NIF ?? string.Empty;
        Telefone = dto.Telefone ?? string.Empty;
        Email = dto.Email ?? string.Empty;
        Morada = dto.Morada ?? string.Empty;
        Moeda = dto.Moeda;
        Mensagem = "Empresa carregada para edição.";
    }

    private async Task GuardarAsync()
    {
        Mensagem = string.Empty;
        AGuardar = true;
        try
        {
            var result = await _service.SaveAsync(new CompanySaveRequest(
                _idEdicao, Nome, NIF, Morada, Telefone, Email, Moeda));

            if (result.IsFailure)
            {
                Mensagem = ObterErro(result.Errors, result.Message);
                return;
            }

            LimparFormulario();
            Mensagem = result.Message ?? "Empresa guardada com sucesso.";
            await CarregarAsync();
        }
        finally
        {
            AGuardar = false;
        }
    }

    private async Task AlternarAtivoAsync(object? parametro)
    {
        if (parametro is not EmpresaListItemDto empresa) return;

        var result = await _service.SetActiveAsync(empresa.Id, !empresa.Ativo);
        Mensagem = result.IsSuccess
            ? result.Message ?? "Estado atualizado."
            : ObterErro(result.Errors, result.Message);

        if (result.IsSuccess)
            await CarregarAsync();
    }

    private void LimparFormulario()
    {
        _idEdicao = 0;
        Nome = NIF = Telefone = Email = Morada = string.Empty;
        Moeda = "FCFA";
        Mensagem = string.Empty;
    }

    private static string ObterErro(IReadOnlyCollection<string> errors, string? message) =>
        errors.FirstOrDefault() ?? message ?? "Não foi possível concluir a operação.";
}
