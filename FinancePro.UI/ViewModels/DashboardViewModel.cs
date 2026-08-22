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
    private readonly int _utilizadorId;

    private bool _aCarregar;
    private string _empresaNome = string.Empty;
    private int _exercicio;
    private decimal _saldoCaixa;
    private decimal _saldoBancario;
    private decimal _totalReceitas;
    private decimal _totalDespesas;
    private decimal _resultado;
    private decimal _margemPercentual;
    private decimal _totalDisponivel;
    private string _nomeUtilizador = string.Empty;
    private string _ultimaAtualizacao = string.Empty;
    private string _mensagemErro = string.Empty;
    private int _obrigacoesFiscaisAtrasadas;
    private int _obrigacoesFiscaisProximas;
    private int _obrigacoesFiscaisCumpridas;
    private decimal _taxaConformidadeFiscal;
    private decimal _execucaoOrcamentalPercentual;
    private decimal _valorPatrimonio;
    private int _bensAtivos;
    private int _bensEmManutencao;
    private int _workflowPendentes;
    private int _workflowAtrasados;
    private int _workflowUrgentes;
    private decimal _variacaoReceitasPercentual, _variacaoDespesasPercentual, _contasReceberPendente, _contasPagarPendente, _liquidezImediataPercentual, _resultadoProjetos;
    private int _projetosAtivos, _projetosCriticos;

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
    public decimal TotalDisponivel { get => _totalDisponivel; set => SetProperty(ref _totalDisponivel, value); }
    public string NomeUtilizador { get => _nomeUtilizador; set => SetProperty(ref _nomeUtilizador, value); }
    public string UltimaAtualizacao { get => _ultimaAtualizacao; set => SetProperty(ref _ultimaAtualizacao, value); }
    public string MensagemErro { get => _mensagemErro; set => SetProperty(ref _mensagemErro, value); }
    public int ObrigacoesFiscaisAtrasadas { get => _obrigacoesFiscaisAtrasadas; set => SetProperty(ref _obrigacoesFiscaisAtrasadas, value); }
    public int ObrigacoesFiscaisProximas { get => _obrigacoesFiscaisProximas; set => SetProperty(ref _obrigacoesFiscaisProximas, value); }
    public int ObrigacoesFiscaisCumpridas { get => _obrigacoesFiscaisCumpridas; set => SetProperty(ref _obrigacoesFiscaisCumpridas, value); }
    public decimal TaxaConformidadeFiscal { get => _taxaConformidadeFiscal; set => SetProperty(ref _taxaConformidadeFiscal, value); }
    public decimal ExecucaoOrcamentalPercentual { get => _execucaoOrcamentalPercentual; set => SetProperty(ref _execucaoOrcamentalPercentual, value); }
    public decimal ValorPatrimonio { get => _valorPatrimonio; set => SetProperty(ref _valorPatrimonio, value); }
    public int BensAtivos { get => _bensAtivos; set => SetProperty(ref _bensAtivos, value); }
    public int BensEmManutencao { get => _bensEmManutencao; set => SetProperty(ref _bensEmManutencao, value); }
    public int WorkflowPendentes { get => _workflowPendentes; set => SetProperty(ref _workflowPendentes, value); }
    public int WorkflowAtrasados { get => _workflowAtrasados; set => SetProperty(ref _workflowAtrasados, value); }
    public int WorkflowUrgentes { get => _workflowUrgentes; set => SetProperty(ref _workflowUrgentes, value); }
    public decimal VariacaoReceitasPercentual { get => _variacaoReceitasPercentual; set => SetProperty(ref _variacaoReceitasPercentual, value); }
    public decimal VariacaoDespesasPercentual { get => _variacaoDespesasPercentual; set => SetProperty(ref _variacaoDespesasPercentual, value); }
    public decimal ContasReceberPendente { get => _contasReceberPendente; set => SetProperty(ref _contasReceberPendente, value); }
    public decimal ContasPagarPendente { get => _contasPagarPendente; set => SetProperty(ref _contasPagarPendente, value); }
    public decimal LiquidezImediataPercentual { get => _liquidezImediataPercentual; set => SetProperty(ref _liquidezImediataPercentual, value); }
    public decimal ResultadoProjetos { get => _resultadoProjetos; set => SetProperty(ref _resultadoProjetos, value); }
    public int ProjetosAtivos { get => _projetosAtivos; set => SetProperty(ref _projetosAtivos, value); }
    public int ProjetosCriticos { get => _projetosCriticos; set => SetProperty(ref _projetosCriticos, value); }

    public string TermoPesquisa
    {
        get => _termoPesquisa;
        set
        {
            if (SetProperty(ref _termoPesquisa, value))
            {
                _ = PesquisarAsync();
            }
        }
    }

    public bool ResultadosPesquisaVisiveis { get => _resultadosPesquisaVisiveis; set => SetProperty(ref _resultadosPesquisaVisiveis, value); }

    public ObservableCollection<PesquisaResultadoDto> ResultadosPesquisa { get; } = new();
    public ObservableCollection<MovimentoRecenteDto> MovimentosRecentes { get; } = new();
    public ObservableCollection<SaldoOrigemDto> SaldosPorOrigem { get; } = new();
    public ObservableCollection<ContaReceberListItemDto> Pendencias { get; } = new();
    public ObservableCollection<AlertaDto> Alertas { get; } = new();

    /// <summary>Disparado pelos botões de Ações Rápidas — a MainWindow decide para
    /// que módulo navegar (evita o Dashboard depender diretamente da shell).</summary>
    public event Action<string>? NavegarPedido;

    public ICommand NovoMovimentoCommand { get; }
    public ICommand NovaContaReceberCommand { get; }
    public ICommand NovaCaixaCommand { get; }
    public ICommand NovoUtilizadorCommand { get; }
    public ICommand NovaDespesaCommand { get; }
    public ICommand NovoOrcamentoCommand { get; }
    public ICommand NovaTransferenciaCommand { get; }
    public ICommand RecarregarCommand { get; }

    public DashboardViewModel(IDashboardService dashboardService, int empresaId, int utilizadorId, string nomeUtilizador)
    {
        _dashboardService = dashboardService;
        _empresaId = empresaId;
        _utilizadorId = utilizadorId;
        NomeUtilizador = string.IsNullOrWhiteSpace(nomeUtilizador) ? "Utilizador" : nomeUtilizador;

        NovoMovimentoCommand = new AsyncRelayCommand(_ => { NavegarPedido?.Invoke("Tesouraria"); return Task.CompletedTask; });
        NovaContaReceberCommand = new AsyncRelayCommand(_ => { NavegarPedido?.Invoke("Receitas"); return Task.CompletedTask; });
        NovaCaixaCommand = new AsyncRelayCommand(_ => { NavegarPedido?.Invoke("Caixa"); return Task.CompletedTask; });
        NovoUtilizadorCommand = new AsyncRelayCommand(_ => { NavegarPedido?.Invoke("Utilizadores"); return Task.CompletedTask; });
        NovaDespesaCommand = new AsyncRelayCommand(_ => { NavegarPedido?.Invoke("Despesas"); return Task.CompletedTask; });
        NovoOrcamentoCommand = new AsyncRelayCommand(_ => { NavegarPedido?.Invoke("Orcamento"); return Task.CompletedTask; });
        NovaTransferenciaCommand = new AsyncRelayCommand(_ => { NavegarPedido?.Invoke("Tesouraria"); return Task.CompletedTask; });
        RecarregarCommand = new AsyncRelayCommand(_ => CarregarAsync());

        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        ACarregar = true;
        MensagemErro = string.Empty;
        try
        {
            var resumo = await _dashboardService.ObterResumoAsync(_empresaId, _utilizadorId);

            EmpresaNome = resumo.EmpresaNome;
            Exercicio = resumo.Exercicio;
            SaldoCaixa = resumo.SaldoCaixa;
            SaldoBancario = resumo.SaldoBancario;
            TotalReceitas = resumo.TotalReceitas;
            TotalDespesas = resumo.TotalDespesas;
            Resultado = resumo.Resultado;
            MargemPercentual = resumo.MargemPercentual;
            TotalDisponivel = resumo.SaldoTesouraria;
            ObrigacoesFiscaisAtrasadas = resumo.ObrigacoesFiscaisAtrasadas;
            ObrigacoesFiscaisProximas = resumo.ObrigacoesFiscaisProximas;
            ObrigacoesFiscaisCumpridas = resumo.ObrigacoesFiscaisCumpridas;
            TaxaConformidadeFiscal = resumo.TaxaConformidadeFiscal;
            ExecucaoOrcamentalPercentual = resumo.ExecucaoOrcamentalPercentual;
            ValorPatrimonio = resumo.ValorPatrimonio;
            BensAtivos = resumo.BensAtivos;
            BensEmManutencao = resumo.BensEmManutencao;
            WorkflowPendentes = resumo.WorkflowPendentes;
            WorkflowAtrasados = resumo.WorkflowAtrasados;
            WorkflowUrgentes = resumo.WorkflowUrgentes;
            VariacaoReceitasPercentual = resumo.VariacaoReceitasPercentual;
            VariacaoDespesasPercentual = resumo.VariacaoDespesasPercentual;
            ContasReceberPendente = resumo.ContasReceberPendente;
            ContasPagarPendente = resumo.ContasPagarPendente;
            LiquidezImediataPercentual = resumo.LiquidezImediataPercentual;
            ProjetosAtivos = resumo.ProjetosAtivos;
            ProjetosCriticos = resumo.ProjetosCriticos;
            ResultadoProjetos = resumo.ResultadoProjetos;
            UltimaAtualizacao = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            MovimentosRecentes.Clear();
            foreach (var m in resumo.MovimentosRecentes) MovimentosRecentes.Add(m);

            SaldosPorOrigem.Clear();
            foreach (var s in resumo.SaldosPorOrigem) SaldosPorOrigem.Add(s);

            Pendencias.Clear();
            foreach (var p in resumo.Pendencias) Pendencias.Add(p);

            Alertas.Clear();
            foreach (var a in resumo.Alertas) Alertas.Add(a);
        }
        catch (Exception ex)
        {
            MensagemErro = $"Não foi possível atualizar o dashboard: {ex.Message}";
        }
        finally
        {
            ACarregar = false;
        }
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
        ResultadosPesquisa.Clear();
        foreach (var r in resultados) ResultadosPesquisa.Add(r);
        ResultadosPesquisaVisiveis = ResultadosPesquisa.Count > 0;
    }
}
