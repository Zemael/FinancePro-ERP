using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Application.MasterData.Currencies;
using FinancePro.Core.DTOs;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class MoedasViewModel : ViewModelBase
{
    private readonly CurrencyMasterDataService _service;
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
    public ObservableCollection<MoedaDto> MoedasPagina { get; } = new();
    private const int ItensPorPagina = 10;
    private int _paginaAtual = 1;
    public int PaginaAtual { get => _paginaAtual; private set => SetProperty(ref _paginaAtual, value); }
    public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(Moedas.Count / (double)ItensPorPagina));
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
    public ICommand PaginaAnteriorCommand { get; }
    public ICommand PaginaSeguinteCommand { get; }

    public MoedasViewModel(CurrencyMasterDataService service)
    {
        _service = service;
        PesquisarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        NovoCommand = new RelayCommand(_ => Limpar());
        EditarCommand = new RelayCommand(_ => Editar(), _ => Selecionada is not null);
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => !AGuardar);
        AlternarAtivoCommand = new AsyncRelayCommand(AlternarAtivoAsync);
        PaginaAnteriorCommand = new RelayCommand(_ => MudarPagina(-1), _ => PaginaAtual > 1);
        PaginaSeguinteCommand = new RelayCommand(_ => MudarPagina(1), _ => PaginaAtual < TotalPaginas);
        _ = CarregarAsync();
    }

    private void MudarPagina(int delta)
    {
        PaginaAtual += delta;
        AtualizarPagina();
    }

    private void AtualizarPagina()
    {
        MoedasPagina.Clear();
        foreach (var item in Moedas.Skip((PaginaAtual - 1) * ItensPorPagina).Take(ItensPorPagina))
            MoedasPagina.Add(item);
        OnPropertyChanged(nameof(TotalPaginas));
    }

    private async Task CarregarAsync()
    {
        Mensagem = string.Empty;
        var result = await _service.ListAsync(Pesquisa);
        if (result.IsFailure) { Mensagem = string.Join(Environment.NewLine, result.Errors); return; }
        Moedas.Clear();
        foreach (var item in result.Value ?? Array.Empty<MoedaDto>()) Moedas.Add(item);
        PaginaAtual = 1;
        AtualizarPagina();
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
            var result = await _service.SaveAsync(new MoedaDto
            {
                Id = _idEdicao, CodigoIso = CodigoIso, Nome = Nome,
                Simbolo = Simbolo, CasasDecimais = CasasDecimais, Ativo = true
            });
            if (result.IsFailure) { Mensagem = string.Join(Environment.NewLine, result.Errors); return; }
            Limpar();
            Mensagem = result.Message ?? "Moeda guardada com sucesso.";
            await CarregarAsync();
        }
        finally { AGuardar = false; }
    }

    private async Task AlternarAtivoAsync(object? parametro)
    {
        if (parametro is not MoedaDto item) return;
        var result = await _service.SetActiveAsync(item.Id, !item.Ativo);
        Mensagem = result.IsSuccess ? result.Message ?? string.Empty : string.Join(Environment.NewLine, result.Errors);
        if (result.IsSuccess) await CarregarAsync();
    }

    private void Limpar()
    {
        _idEdicao = 0;
        CodigoIso = Nome = Simbolo = string.Empty;
        CasasDecimais = 0;
        Mensagem = string.Empty;
    }
}
