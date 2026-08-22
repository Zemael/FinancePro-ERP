using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Platform.Accounting;
using FinancePro.Platform.Investments;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class InvestimentosViewModel : ViewModelBase
{
    private readonly IAnalyticAccountingService _service;
    private readonly int _empresaId;
    private readonly IInvestmentService _investmentService;
    private readonly List<InvestmentPortfolioRow> _carteiraCompleta = new();
    private string _pesquisa = string.Empty;
    private string _riscoSelecionado = "Todos";
    private string _mensagem = string.Empty;
    private bool _aCarregar;
    private InvestmentPlanRow? _planoSelecionado;
    private string _codigo="",_nome="",_tipo="Equipamentos",_departamento="",_gestor="",_estado="Planeado",_observacoes="";
    private DateTime _dataInicial=DateTime.Today; private DateTime? _dataFinal; private decimal _orcamento,_valorExecutado,_retornoEsperado;
    private decimal _execucaoFisica,_valorFinanciado; private string _fonteFinanciamento="",_proximaEtapa=""; private DateTime? _dataProximaEtapa;
    private string _nivelRisco="Médio",_descricaoRisco="",_planoMitigacao=""; private decimal _roiAlvo; private DateTime? _dataMeta;
    private InvestmentFinancingRow? _financiamentoSelecionado; private string _contrato="",_financiador="",_modalidade="Empréstimo",_estadoFinanciamento="Planeado",_notasFinanciamento=""; private decimal _valorAprovado,_valorDesembolsado,_saldoDevedor,_taxaAnual,_prestacao; private int _prazoMeses=12; private DateTime _inicioFinanciamento=DateTime.Today; private DateTime? _primeiroVencimento;
    private InvestmentFinancingPaymentRow? _prestacaoSelecionada; private int _numeroPrestacao=1; private DateTime _vencimentoPrestacao=DateTime.Today; private decimal _capitalPrestacao,_jurosPrestacao,_valorPrestacao,_valorPagoPrestacao; private DateTime? _dataPagamentoPrestacao; private string _estadoPrestacao="Pendente",_referenciaPagamento="",_notasPrestacao="";
    private InvestmentCashFlowRow? _fluxoSelecionado; private DateTime _dataFluxo=DateTime.Today; private string _descricaoFluxo="",_tipoFluxo="Previsto",_referenciaFluxo="",_notasFluxo=""; private decimal _entradaFluxo,_saidaFluxo,_taxaDesconto=10m;
    private InvestmentEvaluationRow? _avaliacaoSelecionada; private DateTime _dataAvaliacao=DateTime.Today; private string _tipoAvaliacao="Intercalar",_classificacaoFinal="Satisfatório",_recomendacaoAvaliacao="",_estadoAvaliacao="Em análise",_notasAvaliacao=""; private decimal _beneficiosRealizados,_custosAdicionais,_valorResidual,_realizacaoObjetivos;

    public ObservableCollection<InvestmentPortfolioRow> Investimentos { get; } = new();
    public ObservableCollection<InvestmentPlanRow> Planos { get; } = new();
    public ObservableCollection<InvestmentFinancingRow> Financiamentos { get; } = new();
    public ObservableCollection<InvestmentFinancingPaymentRow> Prestacoes { get; } = new();
    public ObservableCollection<InvestmentCashFlowRow> FluxosCaixa { get; } = new();
    public ObservableCollection<InvestmentEvaluationRow> Avaliacoes { get; } = new();
    public IReadOnlyList<string> Modalidades { get; } = ["Empréstimo","Leasing","Linha de crédito","Subvenção","Capital próprio","Outro"];
    public IReadOnlyList<string> TiposFluxo { get; } = ["Previsto","Realizado"];
    public IReadOnlyList<string> TiposAvaliacao { get; } = ["Intercalar","Final","Pós-investimento"];
    public IReadOnlyList<string> ClassificacoesAvaliacao { get; } = ["Excelente","Bom","Satisfatório","Insatisfatório","Crítico"];
    public IReadOnlyList<string> EstadosAvaliacao { get; } = ["Em análise","Validada","Aprovada"];
    public IReadOnlyList<string> Tipos { get; } = ["Equipamentos","Mobiliário","Veículos","Infraestruturas","Tecnologia","Diversos"];
    public IReadOnlyList<string> Riscos { get; } = ["Todos", "Baixo", "Médio", "Alto"];
    public IReadOnlyList<string> NiveisRisco { get; } = ["Baixo", "Médio", "Alto", "Crítico"];
    public ICommand AtualizarCommand { get; }
    public ICommand NovoCommand { get; } public ICommand GuardarCommand { get; } public ICommand AprovarCommand { get; } public ICommand ConcluirCommand { get; } public ICommand ArquivarCommand { get; }
    public ICommand NovoFinanciamentoCommand{get;} public ICommand GuardarFinanciamentoCommand{get;} public ICommand ArquivarFinanciamentoCommand{get;}
    public ICommand GerarPlanoPrestacoesCommand{get;} public ICommand NovaPrestacaoCommand{get;} public ICommand GuardarPrestacaoCommand{get;} public ICommand ArquivarPrestacaoCommand{get;}
    public ICommand NovoFluxoCommand{get;} public ICommand GuardarFluxoCommand{get;} public ICommand ArquivarFluxoCommand{get;}
    public ICommand NovaAvaliacaoCommand{get;} public ICommand GuardarAvaliacaoCommand{get;} public ICommand ArquivarAvaliacaoCommand{get;}
    public InvestmentPlanRow? PlanoSelecionado { get=>_planoSelecionado; set { if(SetProperty(ref _planoSelecionado,value)){if(value is not null)CarregarFormulario(value);_=CarregarFinanciamentosAsync();_=CarregarFluxosAsync();_=CarregarAvaliacoesAsync();} } }
    public InvestmentFinancingRow? FinanciamentoSelecionado{get=>_financiamentoSelecionado;set{if(SetProperty(ref _financiamentoSelecionado,value)){if(value is not null)CarregarFinanciamento(value);_=CarregarPrestacoesAsync();}}}
    public InvestmentFinancingPaymentRow? PrestacaoSelecionada{get=>_prestacaoSelecionada;set{if(SetProperty(ref _prestacaoSelecionada,value)&&value is not null)CarregarPrestacao(value);}}
    public InvestmentCashFlowRow? FluxoSelecionado{get=>_fluxoSelecionado;set{if(SetProperty(ref _fluxoSelecionado,value)&&value is not null)CarregarFluxo(value);}}
    public InvestmentEvaluationRow? AvaliacaoSelecionada{get=>_avaliacaoSelecionada;set{if(SetProperty(ref _avaliacaoSelecionada,value)&&value is not null)CarregarAvaliacao(value);}}
    public string Contrato{get=>_contrato;set=>SetProperty(ref _contrato,value);} public string Financiador{get=>_financiador;set=>SetProperty(ref _financiador,value);} public string Modalidade{get=>_modalidade;set=>SetProperty(ref _modalidade,value);} public string EstadoFinanciamento{get=>_estadoFinanciamento;set=>SetProperty(ref _estadoFinanciamento,value);} public string NotasFinanciamento{get=>_notasFinanciamento;set=>SetProperty(ref _notasFinanciamento,value);} public decimal ValorAprovado{get=>_valorAprovado;set=>SetProperty(ref _valorAprovado,value);} public decimal ValorDesembolsado{get=>_valorDesembolsado;set=>SetProperty(ref _valorDesembolsado,value);} public decimal SaldoDevedor{get=>_saldoDevedor;set=>SetProperty(ref _saldoDevedor,value);} public decimal TaxaAnual{get=>_taxaAnual;set=>SetProperty(ref _taxaAnual,value);} public decimal Prestacao{get=>_prestacao;set=>SetProperty(ref _prestacao,value);} public int PrazoMeses{get=>_prazoMeses;set=>SetProperty(ref _prazoMeses,value);} public DateTime InicioFinanciamento{get=>_inicioFinanciamento;set=>SetProperty(ref _inicioFinanciamento,value);} public DateTime? PrimeiroVencimento{get=>_primeiroVencimento;set=>SetProperty(ref _primeiroVencimento,value);}
    public int NumeroPrestacao{get=>_numeroPrestacao;set=>SetProperty(ref _numeroPrestacao,value);} public DateTime VencimentoPrestacao{get=>_vencimentoPrestacao;set=>SetProperty(ref _vencimentoPrestacao,value);} public decimal CapitalPrestacao{get=>_capitalPrestacao;set=>SetProperty(ref _capitalPrestacao,value);} public decimal JurosPrestacao{get=>_jurosPrestacao;set=>SetProperty(ref _jurosPrestacao,value);} public decimal ValorPrestacao{get=>_valorPrestacao;set=>SetProperty(ref _valorPrestacao,value);} public decimal ValorPagoPrestacao{get=>_valorPagoPrestacao;set=>SetProperty(ref _valorPagoPrestacao,value);} public DateTime? DataPagamentoPrestacao{get=>_dataPagamentoPrestacao;set=>SetProperty(ref _dataPagamentoPrestacao,value);} public string EstadoPrestacao{get=>_estadoPrestacao;set=>SetProperty(ref _estadoPrestacao,value);} public string ReferenciaPagamento{get=>_referenciaPagamento;set=>SetProperty(ref _referenciaPagamento,value);} public string NotasPrestacao{get=>_notasPrestacao;set=>SetProperty(ref _notasPrestacao,value);}
    public DateTime DataFluxo{get=>_dataFluxo;set=>SetProperty(ref _dataFluxo,value);} public string DescricaoFluxo{get=>_descricaoFluxo;set=>SetProperty(ref _descricaoFluxo,value);} public string TipoFluxo{get=>_tipoFluxo;set=>SetProperty(ref _tipoFluxo,value);} public string ReferenciaFluxo{get=>_referenciaFluxo;set=>SetProperty(ref _referenciaFluxo,value);} public string NotasFluxo{get=>_notasFluxo;set=>SetProperty(ref _notasFluxo,value);} public decimal EntradaFluxo{get=>_entradaFluxo;set=>SetProperty(ref _entradaFluxo,value);} public decimal SaidaFluxo{get=>_saidaFluxo;set=>SetProperty(ref _saidaFluxo,value);} public decimal TaxaDesconto{get=>_taxaDesconto;set{if(SetProperty(ref _taxaDesconto,value))NotificarIndicadoresFluxo();}}
    public DateTime DataAvaliacao{get=>_dataAvaliacao;set=>SetProperty(ref _dataAvaliacao,value);} public string TipoAvaliacao{get=>_tipoAvaliacao;set=>SetProperty(ref _tipoAvaliacao,value);} public string ClassificacaoFinal{get=>_classificacaoFinal;set=>SetProperty(ref _classificacaoFinal,value);} public string RecomendacaoAvaliacao{get=>_recomendacaoAvaliacao;set=>SetProperty(ref _recomendacaoAvaliacao,value);} public string EstadoAvaliacao{get=>_estadoAvaliacao;set=>SetProperty(ref _estadoAvaliacao,value);} public string NotasAvaliacao{get=>_notasAvaliacao;set=>SetProperty(ref _notasAvaliacao,value);} public decimal BeneficiosRealizados{get=>_beneficiosRealizados;set=>SetProperty(ref _beneficiosRealizados,value);} public decimal CustosAdicionais{get=>_custosAdicionais;set=>SetProperty(ref _custosAdicionais,value);} public decimal ValorResidual{get=>_valorResidual;set=>SetProperty(ref _valorResidual,value);} public decimal RealizacaoObjetivos{get=>_realizacaoObjetivos;set=>SetProperty(ref _realizacaoObjetivos,value);}
    public string Codigo{get=>_codigo;set=>SetProperty(ref _codigo,value);} public string Nome{get=>_nome;set=>SetProperty(ref _nome,value);} public string Tipo{get=>_tipo;set=>SetProperty(ref _tipo,value);} public string Departamento{get=>_departamento;set=>SetProperty(ref _departamento,value);} public string Gestor{get=>_gestor;set=>SetProperty(ref _gestor,value);} public string Estado{get=>_estado;set=>SetProperty(ref _estado,value);} public string Observacoes{get=>_observacoes;set=>SetProperty(ref _observacoes,value);}
    public DateTime DataInicial{get=>_dataInicial;set=>SetProperty(ref _dataInicial,value);} public DateTime? DataFinal{get=>_dataFinal;set=>SetProperty(ref _dataFinal,value);} public decimal Orcamento{get=>_orcamento;set=>SetProperty(ref _orcamento,value);} public decimal ValorExecutado{get=>_valorExecutado;set=>SetProperty(ref _valorExecutado,value);} public decimal RetornoEsperado{get=>_retornoEsperado;set=>SetProperty(ref _retornoEsperado,value);}
    public decimal ExecucaoFisica{get=>_execucaoFisica;set=>SetProperty(ref _execucaoFisica,value);} public string FonteFinanciamento{get=>_fonteFinanciamento;set=>SetProperty(ref _fonteFinanciamento,value);} public decimal ValorFinanciado{get=>_valorFinanciado;set=>SetProperty(ref _valorFinanciado,value);} public string ProximaEtapa{get=>_proximaEtapa;set=>SetProperty(ref _proximaEtapa,value);} public DateTime? DataProximaEtapa{get=>_dataProximaEtapa;set=>SetProperty(ref _dataProximaEtapa,value);}
    public string NivelRisco{get=>_nivelRisco;set=>SetProperty(ref _nivelRisco,value);} public string DescricaoRisco{get=>_descricaoRisco;set=>SetProperty(ref _descricaoRisco,value);} public string PlanoMitigacao{get=>_planoMitigacao;set=>SetProperty(ref _planoMitigacao,value);} public decimal RoiAlvo{get=>_roiAlvo;set=>SetProperty(ref _roiAlvo,value);} public DateTime? DataMeta{get=>_dataMeta;set=>SetProperty(ref _dataMeta,value);}

    public string Pesquisa
    {
        get => _pesquisa;
        set { if (SetProperty(ref _pesquisa, value)) AplicarFiltros(); }
    }

    public string RiscoSelecionado
    {
        get => _riscoSelecionado;
        set { if (SetProperty(ref _riscoSelecionado, value)) AplicarFiltros(); }
    }

    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }
    public decimal InvestimentoPlaneado => Investimentos.Sum(item => item.PlannedInvestment);
    public decimal InvestimentoExecutado => Investimentos.Sum(item => item.InvestedAmount);
    public decimal RetornoReal => Investimentos.Sum(item => item.ActualReturn);
    public decimal ResultadoTotal => Investimentos.Sum(item => item.Result);
    public decimal RoiCarteira => InvestimentoExecutado == 0 ? 0 : ResultadoTotal / InvestimentoExecutado * 100m;
    public int InvestimentosAtivos => Investimentos.Count(item => item.Status is not "Concluído" and not "Cancelado");
    public int RiscosAltos => Investimentos.Count(item => item.Risk == "Alto");
    public decimal TotalEntradasFluxo=>FluxosCaixa.Sum(x=>x.InflowAmount); public decimal TotalSaidasFluxo=>FluxosCaixa.Sum(x=>x.OutflowAmount); public decimal FluxoLiquido=>TotalEntradasFluxo-TotalSaidasFluxo;
    public decimal VplFluxo{get{if(FluxosCaixa.Count==0||TaxaDesconto<=-100)return 0;var origin=FluxosCaixa.Min(x=>x.FlowDate);return FluxosCaixa.Sum(x=>x.NetAmount/(decimal)Math.Pow(1+(double)TaxaDesconto/100d,(x.FlowDate-origin).TotalDays/365d));}}
    public DateTime? DataPayback{get{decimal accumulated=0;var negative=false;foreach(var x in FluxosCaixa.OrderBy(x=>x.FlowDate)){accumulated+=x.NetAmount;if(accumulated<0)negative=true;if(negative&&accumulated>=0)return x.FlowDate;}return null;}}

    public InvestimentosViewModel(IAnalyticAccountingService service, IInvestmentService investmentService, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;
        _investmentService = investmentService;
        AtualizarCommand = new AsyncRelayCommand(_ => CarregarAsync(), _ => !ACarregar);
        NovoCommand=new AsyncRelayCommand(_=>{LimparFormulario();return Task.CompletedTask;}); GuardarCommand=new AsyncRelayCommand(_=>GuardarAsync()); AprovarCommand=new AsyncRelayCommand(_=>AlterarEstadoAsync("Aprovado")); ConcluirCommand=new AsyncRelayCommand(_=>AlterarEstadoAsync("Concluído")); ArquivarCommand=new AsyncRelayCommand(_=>ArquivarAsync());
        NovoFinanciamentoCommand=new AsyncRelayCommand(_=>{LimparFinanciamento();return Task.CompletedTask;});GuardarFinanciamentoCommand=new AsyncRelayCommand(_=>GuardarFinanciamentoAsync());ArquivarFinanciamentoCommand=new AsyncRelayCommand(_=>ArquivarFinanciamentoAsync());
        GerarPlanoPrestacoesCommand=new AsyncRelayCommand(_=>GerarPlanoPrestacoesAsync());NovaPrestacaoCommand=new AsyncRelayCommand(_=>{LimparPrestacao();return Task.CompletedTask;});GuardarPrestacaoCommand=new AsyncRelayCommand(_=>GuardarPrestacaoAsync());ArquivarPrestacaoCommand=new AsyncRelayCommand(_=>ArquivarPrestacaoAsync());
        NovoFluxoCommand=new AsyncRelayCommand(_=>{LimparFluxo();return Task.CompletedTask;});GuardarFluxoCommand=new AsyncRelayCommand(_=>GuardarFluxoAsync());ArquivarFluxoCommand=new AsyncRelayCommand(_=>ArquivarFluxoAsync());
        NovaAvaliacaoCommand=new AsyncRelayCommand(_=>{LimparAvaliacao();return Task.CompletedTask;});GuardarAvaliacaoCommand=new AsyncRelayCommand(_=>GuardarAvaliacaoAsync());ArquivarAvaliacaoCommand=new AsyncRelayCommand(_=>ArquivarAvaliacaoAsync());
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        ACarregar = true;
        Mensagem = string.Empty;
        try
        {
            var projects = await _service.ListProjectsAsync(_empresaId);
            var plans = await _investmentService.ListAsync(_empresaId);
            _carteiraCompleta.Clear();
            _carteiraCompleta.AddRange(projects.Select(MapearInvestimento));
            Planos.Clear(); foreach(var plan in plans) Planos.Add(plan);
            await CarregarFinanciamentosAsync();
            await CarregarFluxosAsync();
            await CarregarAvaliacoesAsync();
            AplicarFiltros();
            Mensagem = $"{_carteiraCompleta.Count} investimento(s) analisado(s).";
        }
        catch (Exception ex)
        {
            _carteiraCompleta.Clear();
            Investimentos.Clear();
            Mensagem = $"Não foi possível carregar a carteira: {ex.Message}";
            NotificarIndicadores();
        }
        finally
        {
            ACarregar = false;
        }
    }

    private async Task GuardarAsync(){try{await _investmentService.SaveAsync(new(_empresaId,PlanoSelecionado?.Id,Codigo,Nome,Tipo,Departamento,Gestor,DataInicial,DataFinal,Orcamento,ValorExecutado,RetornoEsperado,Estado,Observacoes,ExecucaoFisica,FonteFinanciamento,ValorFinanciado,ProximaEtapa,DataProximaEtapa,NivelRisco,DescricaoRisco,PlanoMitigacao,RoiAlvo,DataMeta));Mensagem="Investimento guardado.";LimparFormulario();await CarregarAsync();}catch(Exception ex){Mensagem=ex.Message;}}
    private async Task AlterarEstadoAsync(string status){if(PlanoSelecionado is null){Mensagem="Selecione um investimento.";return;}try{await _investmentService.SetStatusAsync(_empresaId,PlanoSelecionado.Id,status);Mensagem=$"Investimento atualizado para {status}.";await CarregarAsync();}catch(Exception ex){Mensagem=ex.Message;}}
    private async Task ArquivarAsync(){if(PlanoSelecionado is null){Mensagem="Selecione um investimento.";return;}try{await _investmentService.ArchiveAsync(_empresaId,PlanoSelecionado.Id);Mensagem="Investimento arquivado.";LimparFormulario();await CarregarAsync();}catch(Exception ex){Mensagem=ex.Message;}}
    private void CarregarFormulario(InvestmentPlanRow x){Codigo=x.Code;Nome=x.Name;Tipo=x.Type;Departamento=x.Department;Gestor=x.Manager;DataInicial=x.StartDate;DataFinal=x.EndDate;Orcamento=x.Budget;ValorExecutado=x.ExecutedValue;RetornoEsperado=x.ExpectedReturn;Estado=x.Status;Observacoes=x.Notes;ExecucaoFisica=x.PhysicalProgress;FonteFinanciamento=x.FundingSource;ValorFinanciado=x.FinancedAmount;ProximaEtapa=x.NextMilestone;DataProximaEtapa=x.NextMilestoneDate;NivelRisco=x.RiskLevel;DescricaoRisco=x.RiskDescription;PlanoMitigacao=x.MitigationPlan;RoiAlvo=x.TargetRoi;DataMeta=x.TargetCompletionDate;}
    private void LimparFormulario(){PlanoSelecionado=null;Codigo=Nome=Departamento=Gestor=Observacoes=FonteFinanciamento=ProximaEtapa=DescricaoRisco=PlanoMitigacao="";Tipo="Equipamentos";Estado="Planeado";NivelRisco="Médio";DataInicial=DateTime.Today;DataFinal=DataProximaEtapa=DataMeta=null;Orcamento=ValorExecutado=RetornoEsperado=ExecucaoFisica=ValorFinanciado=RoiAlvo=0;}
    private async Task CarregarFinanciamentosAsync(){try{var rows=await _investmentService.ListFinancingsAsync(_empresaId,PlanoSelecionado?.Id);Financiamentos.Clear();foreach(var row in rows)Financiamentos.Add(row);}catch(Exception ex){Mensagem=ex.Message;}}
    private async Task GuardarFinanciamentoAsync(){if(PlanoSelecionado is null){Mensagem="Selecione o investimento financiado.";return;}try{await _investmentService.SaveFinancingAsync(new(_empresaId,FinanciamentoSelecionado?.Id,PlanoSelecionado.Id,Contrato,Financiador,Modalidade,ValorAprovado,ValorDesembolsado,SaldoDevedor,TaxaAnual,PrazoMeses,Prestacao,InicioFinanciamento,PrimeiroVencimento,EstadoFinanciamento,NotasFinanciamento));Mensagem="Financiamento guardado.";LimparFinanciamento();await CarregarFinanciamentosAsync();}catch(Exception ex){Mensagem=ex.Message;}}
    private async Task ArquivarFinanciamentoAsync(){if(FinanciamentoSelecionado is null){Mensagem="Selecione um financiamento.";return;}try{await _investmentService.ArchiveFinancingAsync(_empresaId,FinanciamentoSelecionado.Id);LimparFinanciamento();await CarregarFinanciamentosAsync();Mensagem="Financiamento arquivado.";}catch(Exception ex){Mensagem=ex.Message;}}
    private void CarregarFinanciamento(InvestmentFinancingRow x){Contrato=x.ContractNumber;Financiador=x.Lender;Modalidade=x.FinancingType;ValorAprovado=x.ApprovedAmount;ValorDesembolsado=x.DisbursedAmount;SaldoDevedor=x.OutstandingBalance;TaxaAnual=x.AnnualRate;PrazoMeses=x.TermMonths;Prestacao=x.InstallmentAmount;InicioFinanciamento=x.StartDate;PrimeiroVencimento=x.FirstDueDate;EstadoFinanciamento=x.Status;NotasFinanciamento=x.Notes;}
    private void LimparFinanciamento(){FinanciamentoSelecionado=null;Contrato=Financiador=NotasFinanciamento="";Modalidade="Empréstimo";EstadoFinanciamento="Planeado";ValorAprovado=ValorDesembolsado=SaldoDevedor=TaxaAnual=Prestacao=0;PrazoMeses=12;InicioFinanciamento=DateTime.Today;PrimeiroVencimento=null;}
    private async Task CarregarPrestacoesAsync(){Prestacoes.Clear();if(FinanciamentoSelecionado is null)return;try{var rows=await _investmentService.ListFinancingPaymentsAsync(_empresaId,FinanciamentoSelecionado.Id);foreach(var row in rows)Prestacoes.Add(row);}catch(Exception ex){Mensagem=ex.Message;}}
    private async Task GerarPlanoPrestacoesAsync(){if(FinanciamentoSelecionado is null){Mensagem="Selecione um financiamento.";return;}try{await _investmentService.GeneratePaymentScheduleAsync(_empresaId,FinanciamentoSelecionado.Id);await CarregarPrestacoesAsync();Mensagem="Plano de prestações gerado.";}catch(Exception ex){Mensagem=ex.Message;}}
    private async Task GuardarPrestacaoAsync(){if(FinanciamentoSelecionado is null){Mensagem="Selecione um financiamento.";return;}try{await _investmentService.SaveFinancingPaymentAsync(new(_empresaId,PrestacaoSelecionada?.Id,FinanciamentoSelecionado.Id,NumeroPrestacao,VencimentoPrestacao,CapitalPrestacao,JurosPrestacao,ValorPrestacao,ValorPagoPrestacao,DataPagamentoPrestacao,EstadoPrestacao,ReferenciaPagamento,NotasPrestacao));LimparPrestacao();await CarregarPrestacoesAsync();await CarregarFinanciamentosAsync();Mensagem="Prestação guardada e saldo atualizado.";}catch(Exception ex){Mensagem=ex.Message;}}
    private async Task ArquivarPrestacaoAsync(){if(PrestacaoSelecionada is null){Mensagem="Selecione uma prestação.";return;}try{await _investmentService.ArchiveFinancingPaymentAsync(_empresaId,PrestacaoSelecionada.Id);LimparPrestacao();await CarregarPrestacoesAsync();Mensagem="Prestação arquivada.";}catch(Exception ex){Mensagem=ex.Message;}}
    private void CarregarPrestacao(InvestmentFinancingPaymentRow x){NumeroPrestacao=x.InstallmentNumber;VencimentoPrestacao=x.DueDate;CapitalPrestacao=x.PrincipalAmount;JurosPrestacao=x.InterestAmount;ValorPrestacao=x.AmountDue;ValorPagoPrestacao=x.AmountPaid;DataPagamentoPrestacao=x.PaymentDate;EstadoPrestacao=x.Status;ReferenciaPagamento=x.PaymentReference;NotasPrestacao=x.Notes;}
    private void LimparPrestacao(){PrestacaoSelecionada=null;NumeroPrestacao=Prestacoes.Count+1;VencimentoPrestacao=DateTime.Today;CapitalPrestacao=JurosPrestacao=ValorPrestacao=ValorPagoPrestacao=0;DataPagamentoPrestacao=null;EstadoPrestacao="Pendente";ReferenciaPagamento=NotasPrestacao="";}
    private async Task CarregarFluxosAsync(){FluxosCaixa.Clear();if(PlanoSelecionado is null){NotificarIndicadoresFluxo();return;}try{var rows=await _investmentService.ListCashFlowsAsync(_empresaId,PlanoSelecionado.Id);foreach(var row in rows)FluxosCaixa.Add(row);NotificarIndicadoresFluxo();}catch(Exception ex){Mensagem=ex.Message;}}
    private async Task GuardarFluxoAsync(){if(PlanoSelecionado is null){Mensagem="Selecione um investimento.";return;}try{await _investmentService.SaveCashFlowAsync(new(_empresaId,FluxoSelecionado?.Id,PlanoSelecionado.Id,DataFluxo,DescricaoFluxo,TipoFluxo,EntradaFluxo,SaidaFluxo,ReferenciaFluxo,NotasFluxo));LimparFluxo();await CarregarFluxosAsync();Mensagem="Fluxo de caixa guardado.";}catch(Exception ex){Mensagem=ex.Message;}}
    private async Task ArquivarFluxoAsync(){if(FluxoSelecionado is null){Mensagem="Selecione um fluxo de caixa.";return;}try{await _investmentService.ArchiveCashFlowAsync(_empresaId,FluxoSelecionado.Id);LimparFluxo();await CarregarFluxosAsync();Mensagem="Fluxo de caixa arquivado.";}catch(Exception ex){Mensagem=ex.Message;}}
    private void CarregarFluxo(InvestmentCashFlowRow x){DataFluxo=x.FlowDate;DescricaoFluxo=x.Description;TipoFluxo=x.FlowType;EntradaFluxo=x.InflowAmount;SaidaFluxo=x.OutflowAmount;ReferenciaFluxo=x.Reference;NotasFluxo=x.Notes;}
    private void LimparFluxo(){FluxoSelecionado=null;DataFluxo=DateTime.Today;DescricaoFluxo=ReferenciaFluxo=NotasFluxo="";TipoFluxo="Previsto";EntradaFluxo=SaidaFluxo=0;}
    private void NotificarIndicadoresFluxo(){foreach(var property in new[]{nameof(TotalEntradasFluxo),nameof(TotalSaidasFluxo),nameof(FluxoLiquido),nameof(VplFluxo),nameof(DataPayback)})OnPropertyChanged(property);}
    private async Task CarregarAvaliacoesAsync(){Avaliacoes.Clear();if(PlanoSelecionado is null)return;try{var rows=await _investmentService.ListEvaluationsAsync(_empresaId,PlanoSelecionado.Id);foreach(var row in rows)Avaliacoes.Add(row);}catch(Exception ex){Mensagem=ex.Message;}}
    private async Task GuardarAvaliacaoAsync(){if(PlanoSelecionado is null){Mensagem="Selecione um investimento.";return;}try{await _investmentService.SaveEvaluationAsync(new(_empresaId,AvaliacaoSelecionada?.Id,PlanoSelecionado.Id,DataAvaliacao,TipoAvaliacao,BeneficiosRealizados,CustosAdicionais,ValorResidual,RealizacaoObjetivos,ClassificacaoFinal,RecomendacaoAvaliacao,EstadoAvaliacao,NotasAvaliacao));LimparAvaliacao();await CarregarAvaliacoesAsync();Mensagem="Avaliação do investimento guardada.";}catch(Exception ex){Mensagem=ex.Message;}}
    private async Task ArquivarAvaliacaoAsync(){if(AvaliacaoSelecionada is null){Mensagem="Selecione uma avaliação.";return;}try{await _investmentService.ArchiveEvaluationAsync(_empresaId,AvaliacaoSelecionada.Id);LimparAvaliacao();await CarregarAvaliacoesAsync();Mensagem="Avaliação arquivada.";}catch(Exception ex){Mensagem=ex.Message;}}
    private void CarregarAvaliacao(InvestmentEvaluationRow x){DataAvaliacao=x.ReviewDate;TipoAvaliacao=x.EvaluationType;BeneficiosRealizados=x.BenefitsRealized;CustosAdicionais=x.AdditionalCosts;ValorResidual=x.ResidualValue;RealizacaoObjetivos=x.ObjectivesAchievement;ClassificacaoFinal=x.FinalRating;RecomendacaoAvaliacao=x.Recommendation;EstadoAvaliacao=x.Status;NotasAvaliacao=x.Notes;}
    private void LimparAvaliacao(){AvaliacaoSelecionada=null;DataAvaliacao=DateTime.Today;TipoAvaliacao="Intercalar";BeneficiosRealizados=CustosAdicionais=ValorResidual=RealizacaoObjetivos=0;ClassificacaoFinal="Satisfatório";RecomendacaoAvaliacao=NotasAvaliacao="";EstadoAvaliacao="Em análise";}

    private void AplicarFiltros()
    {
        var query = _carteiraCompleta.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(Pesquisa))
        {
            var term = Pesquisa.Trim();
            query = query.Where(item =>
                item.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.Client.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (RiscoSelecionado != "Todos")
            query = query.Where(item => item.Risk == RiscoSelecionado);

        Investimentos.Clear();
        foreach (var item in query.OrderByDescending(item => item.InvestedAmount))
            Investimentos.Add(item);
        NotificarIndicadores();
    }

    private static InvestmentPortfolioRow MapearInvestimento(ProjectRow project)
    {
        var roi = project.Cost == 0 ? 0 : project.Result / project.Cost * 100m;
        var execution = project.BudgetCost == 0 ? 0 : project.Cost / project.BudgetCost * 100m;
        var risk = project.BudgetCost > 0 && project.Cost > project.BudgetCost
            ? "Alto"
            : execution >= 80m || roi < 10m ? "Médio" : "Baixo";
        return new InvestmentPortfolioRow(
            project.Id, project.Code, project.Name, project.Client, project.Status,
            project.BudgetCost, project.Cost, project.BudgetRevenue, project.Revenue,
            project.Result, roi, execution, risk);
    }

    private void NotificarIndicadores()
    {
        foreach (var property in new[]
        {
            nameof(InvestimentoPlaneado), nameof(InvestimentoExecutado), nameof(RetornoReal),
            nameof(ResultadoTotal), nameof(RoiCarteira), nameof(InvestimentosAtivos), nameof(RiscosAltos)
        }) OnPropertyChanged(property);
    }
}

public sealed record InvestmentPortfolioRow(
    int Id, string Code, string Name, string Client, string Status,
    decimal PlannedInvestment, decimal InvestedAmount, decimal ExpectedReturn,
    decimal ActualReturn, decimal Result, decimal Roi, decimal Execution, string Risk);
