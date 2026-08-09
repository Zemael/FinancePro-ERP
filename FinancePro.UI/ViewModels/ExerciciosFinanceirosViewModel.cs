using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class ExerciciosFinanceirosViewModel : ViewModelBase
{
    private readonly IExercicioFinanceiroService _service;
    private readonly IEmpresaService _empresaService;
    private string _pesquisa = string.Empty;
    private ExercicioFinanceiroDto? _selecionado;
    private int _idEdicao;
    private int _empresaId;
    private int _ano = DateTime.Today.Year;
    private DateTime _dataInicio = new(DateTime.Today.Year, 1, 1);
    private DateTime _dataFim = new(DateTime.Today.Year, 12, 31);
    private bool _padrao = true;
    private bool _encerrado;
    private string _mensagem = string.Empty;
    private bool _aGuardar;
    private string _motivoReabertura = string.Empty;
    private string _resumoFecho = string.Empty;

    public ObservableCollection<ExercicioFinanceiroDto> Exercicios { get; } = new();
    public ObservableCollection<EmpresaListItemDto> Empresas { get; } = new();
    public string Pesquisa { get => _pesquisa; set => SetProperty(ref _pesquisa, value); }
    public ExercicioFinanceiroDto? Selecionado { get => _selecionado; set => SetProperty(ref _selecionado, value); }
    public int EmpresaId { get => _empresaId; set => SetProperty(ref _empresaId, value); }
    public int Ano { get => _ano; set { if (SetProperty(ref _ano, value) && value is >= 2000 and <= 2200) { DataInicio = new DateTime(value,1,1); DataFim = new DateTime(value,12,31); } } }
    public DateTime DataInicio { get => _dataInicio; set => SetProperty(ref _dataInicio, value); }
    public DateTime DataFim { get => _dataFim; set => SetProperty(ref _dataFim, value); }
    public bool Padrao { get => _padrao; set => SetProperty(ref _padrao, value); }
    public bool Encerrado { get => _encerrado; set => SetProperty(ref _encerrado, value); }
    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }
    public string MotivoReabertura { get => _motivoReabertura; set => SetProperty(ref _motivoReabertura, value); }
    public string ResumoFecho { get => _resumoFecho; set => SetProperty(ref _resumoFecho, value); }

    public ICommand PesquisarCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand AlternarAtivoCommand { get; }
    public ICommand ValidarFechoCommand { get; }
    public ICommand EncerrarExercicioCommand { get; }
    public ICommand ReabrirExercicioCommand { get; }

    public ExerciciosFinanceirosViewModel(IExercicioFinanceiroService service, IEmpresaService empresaService)
    {
        _service = service;
        _empresaService = empresaService;
        PesquisarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        NovoCommand = new RelayCommand(_ => Limpar());
        EditarCommand = new RelayCommand(_ => Editar(), _ => Selecionado is not null);
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => !AGuardar);
        AlternarAtivoCommand = new AsyncRelayCommand(AlternarAtivoAsync);
        ValidarFechoCommand = new AsyncRelayCommand(_ => ValidarFechoAsync(), _ => Selecionado is not null);
        EncerrarExercicioCommand = new AsyncRelayCommand(_ => EncerrarExercicioAsync(), _ => Selecionado is not null);
        ReabrirExercicioCommand = new AsyncRelayCommand(_ => ReabrirExercicioAsync(), _ => Selecionado is not null);
        _ = InicializarAsync();
    }

    private async Task InicializarAsync()
    {
        var empresas = await _empresaService.ListarAsync();
        foreach (var item in empresas.Where(x => x.Ativo)) Empresas.Add(item);
        if (Empresas.Count > 0) EmpresaId = Empresas[0].Id;
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var lista = await _service.ListarAsync(Pesquisa);
        Exercicios.Clear();
        foreach (var item in lista) Exercicios.Add(item);
    }

    private void Editar()
    {
        if (Selecionado is null) return;
        _idEdicao = Selecionado.Id;
        EmpresaId = Selecionado.EmpresaId;
        Ano = Selecionado.Ano;
        DataInicio = Selecionado.DataInicio;
        DataFim = Selecionado.DataFim;
        Padrao = Selecionado.Padrao;
        Encerrado = Selecionado.Encerrado;
        Mensagem = "Exercício carregado para edição.";
    }

    private async Task GuardarAsync()
    {
        AGuardar = true;
        Mensagem = string.Empty;
        try
        {
            await _service.GuardarAsync(new ExercicioFinanceiroDto
            {
                Id = _idEdicao, EmpresaId = EmpresaId, Ano = Ano,
                DataInicio = DataInicio, DataFim = DataFim,
                Padrao = Padrao, Encerrado = Encerrado, Ativo = true
            });
            Limpar();
            Mensagem = "Exercício guardado com sucesso.";
            await CarregarAsync();
        }
        catch (Exception ex) { Mensagem = ex.Message; }
        finally { AGuardar = false; }
    }

    private async Task AlternarAtivoAsync(object? parametro)
    {
        if (parametro is not ExercicioFinanceiroDto item) return;
        await _service.AlternarAtivoAsync(item.Id, !item.Ativo);
        await CarregarAsync();
    }

    private async Task ValidarFechoAsync()
    {
        if (Selecionado is null) return;
        try { var p = await _service.ObterPreviewFechoAsync(Selecionado.Id); ResumoFecho = string.Join("\n", p.Verificacoes.Select(x => $"{(x.Quantidade == 0 ? "✓" : "•")} {x.Descricao}: {x.Mensagem}")); Mensagem = p.PodeEncerrar ? "Exercício pronto para encerramento." : $"Existem {p.PendenciasBloqueantes} pendência(s) bloqueante(s)."; }
        catch (Exception ex) { Mensagem = ex.Message; }
    }

    private async Task EncerrarExercicioAsync()
    {
        if (Selecionado is null) return;
        try { await _service.EncerrarAsync(Selecionado.Id, SessaoAtual.UtilizadorId, SessaoAtual.NomeCompleto); Mensagem = "Exercício encerrado e exercício seguinte preparado com sucesso."; await CarregarAsync(); }
        catch (Exception ex) { Mensagem = ex.Message; }
    }

    private async Task ReabrirExercicioAsync()
    {
        if (Selecionado is null) return;
        try { await _service.ReabrirAsync(Selecionado.Id, SessaoAtual.UtilizadorId, SessaoAtual.NomeCompleto, MotivoReabertura); Mensagem = "Exercício reaberto com sucesso."; MotivoReabertura = string.Empty; await CarregarAsync(); }
        catch (Exception ex) { Mensagem = ex.Message; }
    }

    private void Limpar()
    {
        _idEdicao = 0;
        Ano = DateTime.Today.Year;
        DataInicio = new DateTime(Ano,1,1);
        DataFim = new DateTime(Ano,12,31);
        Padrao = true;
        Encerrado = false;
        Mensagem = string.Empty;
    }
}
