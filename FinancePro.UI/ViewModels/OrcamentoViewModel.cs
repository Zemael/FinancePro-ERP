using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class OrcamentoViewModel : ViewModelBase
{
    private readonly IOrcamentoService _service;
    private readonly int _empresaId;

    // --- Criar orçamento ---
    private string _novoAno = DateTime.Today.Year.ToString();
    private string _novoNome = string.Empty;
    private DateTime _novaDataInicio = new(DateTime.Today.Year, 1, 1);
    private DateTime _novaDataFim = new(DateTime.Today.Year, 12, 31);
    private string _novaMoeda = "FCFA";
    private string _mensagemErroOrcamento = string.Empty;
    private bool _aGuardarOrcamento;

    public string NovoAno { get => _novoAno; set => SetProperty(ref _novoAno, value); }
    public string NovoNome { get => _novoNome; set => SetProperty(ref _novoNome, value); }
    public DateTime NovaDataInicio { get => _novaDataInicio; set => SetProperty(ref _novaDataInicio, value); }
    public DateTime NovaDataFim { get => _novaDataFim; set => SetProperty(ref _novaDataFim, value); }
    public string NovaMoeda { get => _novaMoeda; set => SetProperty(ref _novaMoeda, value); }
    public string MensagemErroOrcamento { get => _mensagemErroOrcamento; set => SetProperty(ref _mensagemErroOrcamento, value); }
    public bool AGuardarOrcamento { get => _aGuardarOrcamento; set => SetProperty(ref _aGuardarOrcamento, value); }
    public ObservableCollection<OrcamentoListItemDto> Orcamentos { get; } = new();
    public ICommand CriarOrcamentoCommand { get; }
    public ICommand SelecionarOrcamentoCommand { get; }

    // --- Orçamento selecionado ---
    private OrcamentoListItemDto? _orcamentoSelecionado;
    public OrcamentoListItemDto? OrcamentoSelecionado
    {
        get => _orcamentoSelecionado;
        set
        {
            if (SetProperty(ref _orcamentoSelecionado, value) && value is not null)
            {
                _ = CarregarDetalheOrcamentoAsync();
            }
        }
    }

    // --- Linhas (Receitas / Despesas) ---
    public ObservableCollection<PlanoContasOpcaoDto> Contas { get; } = new();
    public ObservableCollection<OrcamentoDetalheDto> ReceitasPrevistas { get; } = new();
    public ObservableCollection<OrcamentoDetalheDto> DespesasPrevistas { get; } = new();

    private PlanoContasOpcaoDto? _contaSelecionadaReceita;
    private string _centroCustoReceita = string.Empty;
    private string _departamentoReceita = string.Empty;
    private int _mesReceita = DateTime.Today.Month;
    private string _valorPrevistoReceitaTexto = string.Empty;
    private string _mensagemErroReceita = string.Empty;

    public PlanoContasOpcaoDto? ContaSelecionadaReceita { get => _contaSelecionadaReceita; set => SetProperty(ref _contaSelecionadaReceita, value); }
    public string CentroCustoReceita { get => _centroCustoReceita; set => SetProperty(ref _centroCustoReceita, value); }
    public string DepartamentoReceita { get => _departamentoReceita; set => SetProperty(ref _departamentoReceita, value); }
    public int MesReceita { get => _mesReceita; set => SetProperty(ref _mesReceita, value); }
    public string ValorPrevistoReceitaTexto { get => _valorPrevistoReceitaTexto; set => SetProperty(ref _valorPrevistoReceitaTexto, value); }
    public string MensagemErroReceita { get => _mensagemErroReceita; set => SetProperty(ref _mensagemErroReceita, value); }
    public ICommand AdicionarReceitaCommand { get; }

    private PlanoContasOpcaoDto? _contaSelecionadaDespesa;
    private string _centroCustoDespesa = string.Empty;
    private string _departamentoDespesa = string.Empty;
    private int _mesDespesa = DateTime.Today.Month;
    private string _valorPrevistoDespesaTexto = string.Empty;
    private string _mensagemErroDespesa = string.Empty;

    public PlanoContasOpcaoDto? ContaSelecionadaDespesa { get => _contaSelecionadaDespesa; set => SetProperty(ref _contaSelecionadaDespesa, value); }
    public string CentroCustoDespesa { get => _centroCustoDespesa; set => SetProperty(ref _centroCustoDespesa, value); }
    public string DepartamentoDespesa { get => _departamentoDespesa; set => SetProperty(ref _departamentoDespesa, value); }
    public int MesDespesa { get => _mesDespesa; set => SetProperty(ref _mesDespesa, value); }
    public string ValorPrevistoDespesaTexto { get => _valorPrevistoDespesaTexto; set => SetProperty(ref _valorPrevistoDespesaTexto, value); }
    public string MensagemErroDespesa { get => _mensagemErroDespesa; set => SetProperty(ref _mensagemErroDespesa, value); }
    public ICommand AdicionarDespesaCommand { get; }

    public IReadOnlyList<int> MesesDisponiveis { get; } = Enumerable.Range(1, 12).ToList();

    // --- Execução ---
    public ObservableCollection<ExecucaoMensalDto> Execucao { get; } = new();

    // --- Revisões ---
    public ObservableCollection<RevisaoOrcamentalDto> Revisoes { get; } = new();
    private string _motivoRevisao = string.Empty;
    private string _responsavelRevisao = string.Empty;
    private string _mensagemErroRevisao = string.Empty;

    public string MotivoRevisao { get => _motivoRevisao; set => SetProperty(ref _motivoRevisao, value); }
    public string ResponsavelRevisao { get => _responsavelRevisao; set => SetProperty(ref _responsavelRevisao, value); }
    public string MensagemErroRevisao { get => _mensagemErroRevisao; set => SetProperty(ref _mensagemErroRevisao, value); }
    public ICommand AdicionarRevisaoCommand { get; }

    // --- Relatório ---
    private decimal _totalPrevistoReceitas;
    private decimal _totalRealizadoReceitas;
    private decimal _totalPrevistoDespesas;
    private decimal _totalRealizadoDespesas;
    private decimal _resultadoPrevisto;
    private decimal _resultadoRealizado;

    public decimal TotalPrevistoReceitas { get => _totalPrevistoReceitas; set => SetProperty(ref _totalPrevistoReceitas, value); }
    public decimal TotalRealizadoReceitas { get => _totalRealizadoReceitas; set => SetProperty(ref _totalRealizadoReceitas, value); }
    public decimal TotalPrevistoDespesas { get => _totalPrevistoDespesas; set => SetProperty(ref _totalPrevistoDespesas, value); }
    public decimal TotalRealizadoDespesas { get => _totalRealizadoDespesas; set => SetProperty(ref _totalRealizadoDespesas, value); }
    public decimal ResultadoPrevisto { get => _resultadoPrevisto; set => SetProperty(ref _resultadoPrevisto, value); }
    public decimal ResultadoRealizado { get => _resultadoRealizado; set => SetProperty(ref _resultadoRealizado, value); }

    public OrcamentoViewModel(IOrcamentoService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;

        CriarOrcamentoCommand = new AsyncRelayCommand(_ => CriarOrcamentoAsync(), _ => !AGuardarOrcamento);
        SelecionarOrcamentoCommand = new AsyncRelayCommand(p => { OrcamentoSelecionado = p as OrcamentoListItemDto; return Task.CompletedTask; });
        AdicionarReceitaCommand = new AsyncRelayCommand(_ => AdicionarLinhaAsync(TipoCategoria.Receita));
        AdicionarDespesaCommand = new AsyncRelayCommand(_ => AdicionarLinhaAsync(TipoCategoria.Despesa));
        AdicionarRevisaoCommand = new AsyncRelayCommand(_ => AdicionarRevisaoAsync());

        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var orcamentos = await _service.ListarAsync(_empresaId);
        Orcamentos.Clear();
        foreach (var o in orcamentos) Orcamentos.Add(o);

        var contas = await _service.ListarPlanoContasAsync(_empresaId);
        Contas.Clear();
        foreach (var c in contas) Contas.Add(c);
        ContaSelecionadaReceita = Contas.FirstOrDefault();
        ContaSelecionadaDespesa = Contas.FirstOrDefault();

        OrcamentoSelecionado = Orcamentos.FirstOrDefault();
    }

    private async Task CriarOrcamentoAsync()
    {
        MensagemErroOrcamento = string.Empty;

        if (!int.TryParse(NovoAno, out var ano))
        {
            MensagemErroOrcamento = "Indique um ano válido.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NovoNome))
        {
            MensagemErroOrcamento = "Indique o nome do orçamento.";
            return;
        }

        AGuardarOrcamento = true;
        try
        {
            var id = await _service.CriarAsync(new NovoOrcamentoDto
            {
                Ano = ano,
                Nome = NovoNome,
                DataInicio = NovaDataInicio,
                DataFim = NovaDataFim,
                Moeda = NovaMoeda,
                EmpresaId = _empresaId
            });

            NovoNome = string.Empty;
            await CarregarAsync();
            OrcamentoSelecionado = Orcamentos.FirstOrDefault(o => o.Id == id);
        }
        catch (Exception ex)
        {
            MensagemErroOrcamento = ex.Message;
        }
        finally
        {
            AGuardarOrcamento = false;
        }
    }

    private async Task CarregarDetalheOrcamentoAsync()
    {
        if (OrcamentoSelecionado is null) return;
        var id = OrcamentoSelecionado.Id;

        var receitas = await _service.ListarDetalhesAsync(id, TipoCategoria.Receita);
        ReceitasPrevistas.Clear();
        foreach (var r in receitas) ReceitasPrevistas.Add(r);

        var despesas = await _service.ListarDetalhesAsync(id, TipoCategoria.Despesa);
        DespesasPrevistas.Clear();
        foreach (var d in despesas) DespesasPrevistas.Add(d);

        var execucao = await _service.ObterExecucaoMensalAsync(id);
        Execucao.Clear();
        foreach (var e in execucao) Execucao.Add(e);

        var revisoes = await _service.ListarRevisoesAsync(id);
        Revisoes.Clear();
        foreach (var r in revisoes) Revisoes.Add(r);

        var relatorio = await _service.ObterRelatorioAsync(id);
        TotalPrevistoReceitas = relatorio.TotalPrevistoReceitas;
        TotalRealizadoReceitas = relatorio.TotalRealizadoReceitas;
        TotalPrevistoDespesas = relatorio.TotalPrevistoDespesas;
        TotalRealizadoDespesas = relatorio.TotalRealizadoDespesas;
        ResultadoPrevisto = relatorio.ResultadoPrevisto;
        ResultadoRealizado = relatorio.ResultadoRealizado;
    }

    private async Task AdicionarLinhaAsync(TipoCategoria tipo)
    {
        if (OrcamentoSelecionado is null) return;

        var conta = tipo == TipoCategoria.Receita ? ContaSelecionadaReceita : ContaSelecionadaDespesa;
        var valorTexto = tipo == TipoCategoria.Receita ? ValorPrevistoReceitaTexto : ValorPrevistoDespesaTexto;
        var mes = tipo == TipoCategoria.Receita ? MesReceita : MesDespesa;
        var centroCusto = tipo == TipoCategoria.Receita ? CentroCustoReceita : CentroCustoDespesa;
        var departamento = tipo == TipoCategoria.Receita ? DepartamentoReceita : DepartamentoDespesa;

        void DefinirErro(string msg)
        {
            if (tipo == TipoCategoria.Receita) MensagemErroReceita = msg; else MensagemErroDespesa = msg;
        }

        DefinirErro(string.Empty);

        if (conta is null)
        {
            DefinirErro("Selecione a conta do plano de contas.");
            return;
        }

        if (!decimal.TryParse(valorTexto, out var valor) || valor < 0)
        {
            DefinirErro("Indique um valor previsto válido.");
            return;
        }

        try
        {
            await _service.AdicionarDetalheAsync(new NovoOrcamentoDetalheDto
            {
                OrcamentoId = OrcamentoSelecionado.Id,
                PlanoContasId = conta.Id,
                CentroCusto = centroCusto,
                Departamento = departamento,
                Tipo = tipo,
                Mes = mes,
                ValorPrevisto = valor
            });

            if (tipo == TipoCategoria.Receita) { ValorPrevistoReceitaTexto = string.Empty; }
            else { ValorPrevistoDespesaTexto = string.Empty; }

            await CarregarDetalheOrcamentoAsync();
        }
        catch (Exception ex)
        {
            DefinirErro(ex.Message);
        }
    }

    private async Task AdicionarRevisaoAsync()
    {
        if (OrcamentoSelecionado is null) return;

        MensagemErroRevisao = string.Empty;
        try
        {
            await _service.AdicionarRevisaoAsync(new NovaRevisaoOrcamentalDto
            {
                OrcamentoId = OrcamentoSelecionado.Id,
                Motivo = MotivoRevisao,
                Responsavel = ResponsavelRevisao
            });

            MotivoRevisao = string.Empty;
            ResponsavelRevisao = string.Empty;
            await CarregarDetalheOrcamentoAsync();
        }
        catch (Exception ex)
        {
            MensagemErroRevisao = ex.Message;
        }
    }
}
