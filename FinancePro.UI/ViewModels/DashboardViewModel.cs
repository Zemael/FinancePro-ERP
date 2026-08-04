using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IDashboardService _dashboardService;
    private readonly int _empresaId;

    private bool _aCarregar;
    private string _empresaNome = string.Empty;
    private int _exercicio;
    private decimal _saldoCaixa;
    private decimal _saldoBancario;
    private decimal _totalReceitas;
    private decimal _totalDespesas;
    private decimal _resultado;
    private decimal _margemPercentual;
    private decimal _totalAReceber;
    private decimal _totalAPagar;
    private int _comprasPendentes;
    private decimal _valorPatrimonio;
    private decimal _totalOrcamento;
    private decimal _execucaoOrcamental;
    private int _totalAlertas;
    private string _ultimaAtualizacao = string.Empty;
    private string _termoPesquisa = string.Empty;
    private bool _resultadosPesquisaVisiveis;

    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }
    public string EmpresaNome { get => _empresaNome; set => SetProperty(ref _empresaNome, value); }
    public int Exercicio { get => _exercicio; set => SetProperty(ref _exercicio, value); }
    public decimal SaldoCaixa { get => _saldoCaixa; set => SetProperty(ref _saldoCaixa, value); }
    public decimal SaldoBancario { get => _saldoBancario; set => SetProperty(ref _saldoBancario, value); }
    public decimal TotalReceitas { get => _totalReceitas; set => SetProperty(ref _totalReceitas, value); }
    public decimal TotalDespesas { get => _totalDespesas; set => SetProperty(ref _totalDespesas, value); }
    public decimal Resultado { get => _resultado; set => SetProperty(ref _resultado, value); }
    public decimal MargemPercentual { get => _margemPercentual; set => SetProperty(ref _margemPercentual, value); }
    public decimal TotalAReceber { get => _totalAReceber; set => SetProperty(ref _totalAReceber, value); }
    public decimal TotalAPagar { get => _totalAPagar; set => SetProperty(ref _totalAPagar, value); }
    public int ComprasPendentes { get => _comprasPendentes; set => SetProperty(ref _comprasPendentes, value); }
    public decimal ValorPatrimonio { get => _valorPatrimonio; set => SetProperty(ref _valorPatrimonio, value); }
    public decimal TotalOrcamento { get => _totalOrcamento; set => SetProperty(ref _totalOrcamento, value); }
    public decimal ExecucaoOrcamental { get => _execucaoOrcamental; set => SetProperty(ref _execucaoOrcamental, value); }
    public int TotalAlertas { get => _totalAlertas; set => SetProperty(ref _totalAlertas, value); }
    public string UltimaAtualizacao { get => _ultimaAtualizacao; set => SetProperty(ref _ultimaAtualizacao, value); }

    public string TermoPesquisa
    {
        get => _termoPesquisa;
        set
        {
            if (SetProperty(ref _termoPesquisa, value)) _ = PesquisarAsync();
        }
    }

    public bool ResultadosPesquisaVisiveis { get => _resultadosPesquisaVisiveis; set => SetProperty(ref _resultadosPesquisaVisiveis, value); }

    public ObservableCollection<PesquisaResultadoDto> ResultadosPesquisa { get; } = new();
    public ObservableCollection<MovimentoRecenteDto> MovimentosRecentes { get; } = new();
    public ObservableCollection<SaldoOrigemDto> SaldosPorOrigem { get; } = new();
    public ObservableCollection<ContaReceberListItemDto> Pendencias { get; } = new();
    public ObservableCollection<AlertaDto> Alertas { get; } = new();
    public ObservableCollection<AtividadeRecenteDto> AtividadesRecentes { get; } = new();
    public ObservableCollection<FluxoCaixaMensalDto> FluxoMensal { get; } = new();

    public event Action<string>? NavegarPedido;

    public ICommand AtualizarCommand { get; }
    public ICommand NovoMovimentoCommand { get; }
    public ICommand NovaContaReceberCommand { get; }
    public ICommand NovaDespesaCommand { get; }
    public ICommand NovaCompraCommand { get; }
    public ICommand NovoBemCommand { get; }
    public ICommand NovoOrcamentoCommand { get; }

    public DashboardViewModel(IDashboardService dashboardService, int empresaId)
    {
        _dashboardService = dashboardService;
        _empresaId = empresaId;

        AtualizarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        NovoMovimentoCommand = Navegar("Tesouraria");
        NovaContaReceberCommand = Navegar("Receitas");
        NovaDespesaCommand = Navegar("Despesas");
        NovaCompraCommand = Navegar("Compras");
        NovoBemCommand = Navegar("Património");
        NovoOrcamentoCommand = Navegar("Orçamento");

        _ = CarregarAsync();
    }

    private ICommand Navegar(string modulo) =>
        new AsyncRelayCommand(_ => { NavegarPedido?.Invoke(modulo); return Task.CompletedTask; });

    private async Task CarregarAsync()
    {
        ACarregar = true;
        try
        {
            var resumo = await _dashboardService.ObterResumoAsync(_empresaId);
            EmpresaNome = resumo.EmpresaNome;
            Exercicio = resumo.Exercicio;
            SaldoCaixa = resumo.SaldoCaixa;
            SaldoBancario = resumo.SaldoBancario;
            TotalReceitas = resumo.TotalReceitas;
            TotalDespesas = resumo.TotalDespesas;
            Resultado = resumo.Resultado;
            MargemPercentual = resumo.MargemPercentual;
            TotalAReceber = resumo.TotalAReceber;
            TotalAPagar = resumo.TotalAPagar;
            ComprasPendentes = resumo.ComprasPendentes;
            ValorPatrimonio = resumo.ValorPatrimonio;
            TotalOrcamento = resumo.TotalOrcamento;
            ExecucaoOrcamental = resumo.ExecucaoOrcamental;
            TotalAlertas = resumo.TotalAlertas;
            UltimaAtualizacao = $"Atualizado às {DateTime.Now:HH:mm}";

            Replace(MovimentosRecentes, resumo.MovimentosRecentes);
            Replace(SaldosPorOrigem, resumo.SaldosPorOrigem);
            Replace(Pendencias, resumo.Pendencias);
            Replace(Alertas, resumo.Alertas);
            Replace(AtividadesRecentes, resumo.AtividadesRecentes);
            Replace(FluxoMensal, resumo.FluxoMensal);
        }
        finally
        {
            ACarregar = false;
        }
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items) target.Add(item);
    }

    private async Task PesquisarAsync()
    {
        if (string.IsNullOrWhiteSpace(TermoPesquisa) || TermoPesquisa.Trim().Length < 2)
        {
            ResultadosPesquisa.Clear();
            ResultadosPesquisaVisiveis = false;
            return;
        }

        var resultados = await _dashboardService.PesquisarAsync(_empresaId, TermoPesquisa);
        Replace(ResultadosPesquisa, resultados);
        ResultadosPesquisaVisiveis = ResultadosPesquisa.Count > 0;
    }
}
