using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Application.Administration.Profiles;
using FinancePro.Core.DTOs;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class PerfisViewModel : ViewModelBase
{
    private readonly ProfileAdministrationService _service;
    private string _pesquisa = string.Empty;
    private PerfilDto? _selecionado;
    private int _idEdicao;
    private string _nome = string.Empty;
    private string _descricao = string.Empty;
    private string _mensagem = string.Empty;
    private bool _aGuardar;
    private bool _aCarregar;

    public ObservableCollection<PerfilDto> Perfis { get; } = new();
    public string Pesquisa { get => _pesquisa; set => SetProperty(ref _pesquisa, value); }
    public PerfilDto? Selecionado { get => _selecionado; set => SetProperty(ref _selecionado, value); }
    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }
    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }

    public ICommand PesquisarCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand AlternarAtivoCommand { get; }

    public PerfisViewModel(ProfileAdministrationService service)
    {
        _service = service;
        PesquisarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        NovoCommand = new RelayCommand(_ => Limpar());
        EditarCommand = new RelayCommand(_ => Editar(), _ => Selecionado is not null);
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => !AGuardar);
        AlternarAtivoCommand = new AsyncRelayCommand(AlternarAtivoAsync);
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        ACarregar = true;
        try
        {
            var resultado = await _service.ListAsync(Pesquisa);
            if (resultado.IsFailure)
            {
                Mensagem = ObterErro(resultado.Errors, resultado.Message);
                return;
            }

            Perfis.Clear();
            foreach (var item in resultado.Value ?? Array.Empty<PerfilDto>()) Perfis.Add(item);
            Mensagem = resultado.Message ?? $"{Perfis.Count} perfil(is) carregado(s).";
        }
        finally
        {
            ACarregar = false;
        }
    }

    private void Editar()
    {
        if (Selecionado is null) return;
        _idEdicao = Selecionado.Id;
        Nome = Selecionado.Nome;
        Descricao = Selecionado.Descricao ?? string.Empty;
        Mensagem = "Perfil carregado para edição.";
    }

    private async Task GuardarAsync()
    {
        AGuardar = true;
        Mensagem = string.Empty;
        try
        {
            var resultado = await _service.SaveAsync(new ProfileSaveRequest(
                _idEdicao,
                Nome,
                Descricao,
                true));

            if (resultado.IsFailure)
            {
                Mensagem = ObterErro(resultado.Errors, resultado.Message);
                return;
            }

            var mensagemSucesso = resultado.Message ?? "Perfil guardado com sucesso.";
            Limpar();
            await CarregarAsync();
            Mensagem = mensagemSucesso;
        }
        finally
        {
            AGuardar = false;
        }
    }

    private async Task AlternarAtivoAsync(object? parametro)
    {
        if (parametro is not PerfilDto item) return;
        var resultado = await _service.SetActiveAsync(item.Id, !item.Ativo);
        Mensagem = resultado.IsSuccess
            ? resultado.Message ?? "Estado atualizado."
            : ObterErro(resultado.Errors, resultado.Message);

        if (resultado.IsSuccess) await CarregarAsync();
    }

    private void Limpar()
    {
        _idEdicao = 0;
        Nome = Descricao = string.Empty;
        Selecionado = null;
    }

    private static string ObterErro(IEnumerable<string> erros, string? mensagem) =>
        erros.FirstOrDefault() ?? mensagem ?? "Não foi possível concluir a operação.";
}
