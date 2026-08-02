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

    public ICommand PesquisarCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand AlternarAtivoCommand { get; }

    public ExerciciosFinanceirosViewModel(IExercicioFinanceiroService service, IEmpresaService empresaService)
    {
        _service = service;
        _empresaService = empresaService;
        PesquisarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        NovoCommand = new RelayCommand(_ => Limpar());
        EditarCommand = new RelayCommand(_ => Editar(), _ => Selecionado is not null);
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => !AGuardar);
        AlternarAtivoCommand = new AsyncRelayCommand(AlternarAtivoAsync);
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
