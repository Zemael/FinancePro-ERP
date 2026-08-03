using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class PerfisViewModel : ViewModelBase
{
    private readonly IPerfilService _service;
    private string _pesquisa = string.Empty;
    private PerfilDto? _selecionado;
    private int _idEdicao;
    private string _nome = string.Empty;
    private string _descricao = string.Empty;
    private string _mensagem = string.Empty;
    private bool _aGuardar;

    public ObservableCollection<PerfilDto> Perfis { get; } = new();
    public string Pesquisa { get => _pesquisa; set => SetProperty(ref _pesquisa, value); }
    public PerfilDto? Selecionado { get => _selecionado; set => SetProperty(ref _selecionado, value); }
    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }

    public ICommand PesquisarCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand AlternarAtivoCommand { get; }

    public PerfisViewModel(IPerfilService service)
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
        var lista = await _service.ListarAsync(Pesquisa);
        Perfis.Clear();
        foreach (var item in lista) Perfis.Add(item);
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
            await _service.GuardarAsync(new PerfilDto { Id = _idEdicao, Nome = Nome, Descricao = Descricao, Ativo = true });
            Limpar();
            Mensagem = "Perfil guardado com sucesso.";
            await CarregarAsync();
        }
        catch (Exception ex) { Mensagem = ex.Message; }
        finally { AGuardar = false; }
    }

    private async Task AlternarAtivoAsync(object? parametro)
    {
        if (parametro is not PerfilDto item) return;
        await _service.AlternarAtivoAsync(item.Id, !item.Ativo);
        await CarregarAsync();
    }

    private void Limpar()
    {
        _idEdicao = 0;
        Nome = Descricao = Mensagem = string.Empty;
    }
}
