using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class MoedasViewModel : ViewModelBase
{
    private readonly IMoedaService _service;
    private string _pesquisa = string.Empty;
    private MoedaDto? _selecionada;
    private int _idEdicao;
    private string _codigoIso = string.Empty;
    private string _nome = string.Empty;
    private string _simbolo = string.Empty;
    private int _casasDecimais;
    private string _mensagem = string.Empty;
    private bool _aGuardar;

    public ObservableCollection<MoedaDto> Moedas { get; } = new();
    public string Pesquisa { get => _pesquisa; set => SetProperty(ref _pesquisa, value); }
    public MoedaDto? Selecionada { get => _selecionada; set => SetProperty(ref _selecionada, value); }
    public string CodigoIso { get => _codigoIso; set => SetProperty(ref _codigoIso, value); }
    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string Simbolo { get => _simbolo; set => SetProperty(ref _simbolo, value); }
    public int CasasDecimais { get => _casasDecimais; set => SetProperty(ref _casasDecimais, value); }
    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }

    public ICommand PesquisarCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand AlternarAtivoCommand { get; }

    public MoedasViewModel(IMoedaService service)
    {
        _service = service;
        PesquisarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        NovoCommand = new RelayCommand(_ => Limpar());
        EditarCommand = new RelayCommand(_ => Editar(), _ => Selecionada is not null);
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => !AGuardar);
        AlternarAtivoCommand = new AsyncRelayCommand(AlternarAtivoAsync);
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var lista = await _service.ListarAsync(Pesquisa);
        Moedas.Clear();
        foreach (var item in lista) Moedas.Add(item);
    }

    private void Editar()
    {
        if (Selecionada is null) return;
        _idEdicao = Selecionada.Id;
        CodigoIso = Selecionada.CodigoIso;
        Nome = Selecionada.Nome;
        Simbolo = Selecionada.Simbolo;
        CasasDecimais = Selecionada.CasasDecimais;
        Mensagem = "Moeda carregada para edição.";
    }

    private async Task GuardarAsync()
    {
        AGuardar = true;
        Mensagem = string.Empty;
        try
        {
            await _service.GuardarAsync(new MoedaDto
            {
                Id = _idEdicao, CodigoIso = CodigoIso, Nome = Nome,
                Simbolo = Simbolo, CasasDecimais = CasasDecimais, Ativo = true
            });
            Limpar();
            Mensagem = "Moeda guardada com sucesso.";
            await CarregarAsync();
        }
        catch (Exception ex) { Mensagem = ex.Message; }
        finally { AGuardar = false; }
    }

    private async Task AlternarAtivoAsync(object? parametro)
    {
        if (parametro is not MoedaDto item) return;
        await _service.AlternarAtivoAsync(item.Id, !item.Ativo);
        await CarregarAsync();
    }

    private void Limpar()
    {
        _idEdicao = 0;
        CodigoIso = Nome = Simbolo = string.Empty;
        CasasDecimais = 0;
        Mensagem = string.Empty;
    }
}
