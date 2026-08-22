using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Application.Revenue;
using FinancePro.Core.DTOs;
using FinancePro.UI.Common;
using FinancePro.UI.Services;

namespace FinancePro.UI.ViewModels;

public class ReceitasViewModel : ViewModelBase
{
    private readonly RevenueApplicationService _service;
    private readonly int _empresaId;

    public string Breadcrumb { get; }
    public string TituloModulo { get; }
    public string SubtituloModulo { get; }
    public string TextoCriarDocumento { get; }
    public string TituloLista { get; }
    public bool ModoFaturacao { get; }

    private string _descricao = string.Empty;
    private string _valorTexto = string.Empty;
    private DateTime _dataEmissao = DateTime.Today;
    private DateTime _dataVencimento = DateTime.Today;
    private ClienteOpcaoDto? _clienteSelecionado;
    private CategoriaOpcaoDto? _categoriaSelecionada;
    private string _formaPagamento = string.Empty;
    private string _centroCusto = string.Empty;
    private string _descontoGeralTexto = "0";
    private string _freteTexto = "0";
    private string _outrasDespesasTexto = "0";
    private string _observacoes = string.Empty;
    private string _mensagemErro = string.Empty;
    private string _mensagemSucesso = string.Empty;
    private bool _aGuardar;
    private bool _aCarregar;
    private DateTime _dataRecebimento = DateTime.Today;
    private string _valorRecebimentoTexto = string.Empty;
    private OpcaoOrigemDto? _origemRecebimentoSelecionada;
    private ContaReceberListItemDto? _contaSelecionada; private ProdutoStockDto? _produtoItemSelecionado; private string _quantidadeItem="1", _precoItem="", _descontoItem="0", _ivaItem="0";
    private DocumentoFiscalDto? _documentoFiscalSelecionado;
    private int? _rascunhoIdEdicao;
    private int? _itemIdEdicao;
    private string _pesquisaFaturacao = string.Empty;
    private string _estadoFaturacaoSelecionado = "Todos";
    private DateTime? _dataInicialFaturacao;
    private DateTime? _dataFinalFaturacao;
    private EmpresaDto? _empresaEmitente;

    public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
    public string ValorTexto { get => _valorTexto; set => SetProperty(ref _valorTexto, value); }
    public DateTime DataEmissao { get => _dataEmissao; set => SetProperty(ref _dataEmissao, value); }
    public DateTime DataVencimento { get => _dataVencimento; set => SetProperty(ref _dataVencimento, value); }
    public ClienteOpcaoDto? ClienteSelecionado { get => _clienteSelecionado; set => SetProperty(ref _clienteSelecionado, value); }
    public CategoriaOpcaoDto? CategoriaSelecionada { get => _categoriaSelecionada; set => SetProperty(ref _categoriaSelecionada, value); }
    public string FormaPagamento { get => _formaPagamento; set => SetProperty(ref _formaPagamento, value); }
    public string CentroCusto { get => _centroCusto; set => SetProperty(ref _centroCusto, value); }
    public string DescontoGeralTexto { get => _descontoGeralTexto; set => SetProperty(ref _descontoGeralTexto, value); }
    public string FreteTexto { get => _freteTexto; set => SetProperty(ref _freteTexto, value); }
    public string OutrasDespesasTexto { get => _outrasDespesasTexto; set => SetProperty(ref _outrasDespesasTexto, value); }
    public string Observacoes { get => _observacoes; set => SetProperty(ref _observacoes, value); }
    public string MensagemErro { get => _mensagemErro; set => SetProperty(ref _mensagemErro, value); }
    public string MensagemSucesso { get => _mensagemSucesso; set => SetProperty(ref _mensagemSucesso, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }
    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }
    public string ValorRecebimentoTexto { get => _valorRecebimentoTexto; set => SetProperty(ref _valorRecebimentoTexto, value); }
    public DateTime DataRecebimento { get => _dataRecebimento; set => SetProperty(ref _dataRecebimento, value); }

    public string NotaValor { get => _notaValor; set => SetProperty(ref _notaValor,value); }
    public string NotaMotivo { get => _notaMotivo; set => SetProperty(ref _notaMotivo,value); }
    private string _notaValor = string.Empty; private string _notaMotivo = string.Empty;
    public OpcaoOrigemDto? OrigemRecebimentoSelecionada { get => _origemRecebimentoSelecionada; set => SetProperty(ref _origemRecebimentoSelecionada, value); }
    public ContaReceberListItemDto? ContaSelecionada {get=>_contaSelecionada;set{if(SetProperty(ref _contaSelecionada,value)){_=CarregarItensAsync();_=CarregarDocumentosAsync();}}} public ProdutoStockDto? ProdutoItemSelecionado{get=>_produtoItemSelecionado;set{if(SetProperty(ref _produtoItemSelecionado,value)&&value is not null&&!ItemEmEdicao)PrecoItem=value.PrecoVenda.ToString("N2");}} public string QuantidadeItem{get=>_quantidadeItem;set=>SetProperty(ref _quantidadeItem,value);} public string PrecoItem{get=>_precoItem;set=>SetProperty(ref _precoItem,value);} public string DescontoItem{get=>_descontoItem;set=>SetProperty(ref _descontoItem,value);} public string IvaItem{get=>_ivaItem;set=>SetProperty(ref _ivaItem,value);}
    public DocumentoFiscalDto? DocumentoFiscalSelecionado { get => _documentoFiscalSelecionado; set => SetProperty(ref _documentoFiscalSelecionado, value); }
    public bool EmEdicao => _rascunhoIdEdicao.HasValue;
    public string TextoAcaoDocumento => EmEdicao ? "Guardar alterações" : TextoCriarDocumento;
    public bool ItemEmEdicao => _itemIdEdicao.HasValue;
    public string TextoAcaoItem => ItemEmEdicao ? "Guardar item" : "Adicionar";
    public DocumentoFiscalDto? UltimoRecibo => DocumentosFiscais
        .Where(x => x.Tipo == "Recibo")
        .OrderByDescending(x => x.DataEmissao)
        .ThenByDescending(x => x.Id)
        .FirstOrDefault();
    public bool PodeImprimirUltimoRecibo => UltimoRecibo is not null;
    public int DocumentosEmAberto => Contas.Count(x => x.ComercialEstado == "Faturada" && x.SaldoAberto > 0);
    public decimal ValorFaturado => Contas.Where(x => x.ComercialEstado == "Faturada").Sum(x => x.Valor);
    public decimal ValorRecebido => Contas.Where(x => x.ComercialEstado == "Faturada").Sum(x => x.ValorLiquidado);
    public decimal SaldoPendente => Contas.Where(x => x.ComercialEstado == "Faturada").Sum(x => x.SaldoAberto);
    public string MoedaFaturacao => string.IsNullOrWhiteSpace(_empresaEmitente?.Moeda) ? "FCFA" : _empresaEmitente.Moeda;
    public string ValorFaturadoTexto => $"{ValorFaturado:N0} {MoedaFaturacao}";
    public string ValorRecebidoTexto => $"{ValorRecebido:N0} {MoedaFaturacao}";
    public string SaldoPendenteTexto => $"{SaldoPendente:N0} {MoedaFaturacao}";
    public decimal BaseTributavelDocumento => Itens.Sum(x => x.Subtotal);
    public decimal IvaDocumento => Itens.Sum(x => x.ValorIva);
    public decimal DescontoDocumento => ContaSelecionada?.DescontoGeral ?? 0;
    public decimal EncargosDocumento => (ContaSelecionada?.Frete ?? 0) + (ContaSelecionada?.OutrasDespesas ?? 0);
    public decimal TotalDocumento => ContaSelecionada?.Valor ?? 0;
    public int QuantidadeItensDocumento => Itens.Count;
    public string QuantidadeItensDocumentoTexto => QuantidadeItensDocumento == 1 ? "1 item" : $"{QuantidadeItensDocumento} itens";
    public string BaseTributavelDocumentoTexto => $"{BaseTributavelDocumento:N0} {MoedaFaturacao}";
    public string IvaDocumentoTexto => $"{IvaDocumento:N0} {MoedaFaturacao}";
    public string DescontoDocumentoTexto => $"{DescontoDocumento:N0} {MoedaFaturacao}";
    public string EncargosDocumentoTexto => $"{EncargosDocumento:N0} {MoedaFaturacao}";
    public string TotalDocumentoTexto => $"{TotalDocumento:N0} {MoedaFaturacao}";
    public string PesquisaFaturacao { get => _pesquisaFaturacao; set => SetProperty(ref _pesquisaFaturacao, value); }
    public string EstadoFaturacaoSelecionado { get => _estadoFaturacaoSelecionado; set => SetProperty(ref _estadoFaturacaoSelecionado, value); }
    public DateTime? DataInicialFaturacao { get => _dataInicialFaturacao; set => SetProperty(ref _dataInicialFaturacao, value); }
    public DateTime? DataFinalFaturacao { get => _dataFinalFaturacao; set => SetProperty(ref _dataFinalFaturacao, value); }

    public ObservableCollection<ClienteOpcaoDto> Clientes { get; } = new();
    public ObservableCollection<CategoriaOpcaoDto> Categorias { get; } = new();
    public ObservableCollection<OpcaoOrigemDto> Origens { get; } = new();
    public ObservableCollection<ContaReceberListItemDto> Contas { get; } = new();
    private List<ContaReceberListItemDto> TodasContas { get; } = new();
    public ObservableCollection<string> EstadosFaturacao { get; } = new(new[] { "Todos", "Rascunho", "Proforma validada", "Fatura definitiva", "Parcialmente paga", "Paga", "Vencida", "Cancelada" });
    public ObservableCollection<ProdutoStockDto> ProdutosItens{get;}=new(); public ObservableCollection<DocumentoItemDto> Itens{get;}=new(); public ObservableCollection<DocumentoFiscalDto> DocumentosFiscais{get;}=new();

    public ICommand CriarCommand { get; }
    public ICommand AprovarCommand { get; }
    public ICommand FaturarCommand { get; }
    public ICommand ReceberCommand { get; }
    public ICommand CancelarCommand { get; }
    public ICommand AtualizarCommand { get; } public ICommand AdicionarItemCommand{get;} public ICommand RemoverItemCommand{get;} public ICommand NotaCreditoCommand{get;} public ICommand NotaDebitoCommand{get;}
    public ICommand ImprimirProformaCommand { get; }
    public ICommand ImprimirDocumentoCommand { get; }
    public ICommand ImprimirUltimoReciboCommand { get; }
    public ICommand EditarRascunhoCommand { get; }
    public ICommand CancelarEdicaoCommand { get; }
    public ICommand EditarItemCommand { get; }
    public ICommand CancelarEdicaoItemCommand { get; }
    public ICommand DuplicarDocumentoCommand { get; }
    public ICommand PesquisarFaturacaoCommand { get; }
    public ICommand LimparFiltrosFaturacaoCommand { get; }

    public ReceitasViewModel(RevenueApplicationService service, int empresaId, bool modoFaturacao = false)
    {
        _service = service;
        _empresaId = empresaId;
        ModoFaturacao = modoFaturacao;
        Breadcrumb = modoFaturacao ? "Financeiro / Faturação" : "Financeiro / Receitas";
        TituloModulo = modoFaturacao ? "Faturação" : "Receitas";
        SubtituloModulo = modoFaturacao
            ? "Emissão e gestão de propostas, faturas, recibos, notas de crédito e notas de débito"
            : "Contas a receber de clientes — ao registar o recebimento, gera automaticamente um movimento de Tesouraria";
        TextoCriarDocumento = modoFaturacao ? "Criar fatura proforma" : "Criar proposta";
        TituloLista = modoFaturacao ? "Documentos de faturação" : "Contas a receber";
        ImprimirProformaCommand = new RelayCommand(_ => ImprimirProforma(), _ => ContaSelecionada is not null);
        ImprimirDocumentoCommand = new RelayCommand(ImprimirDocumento, p => ContaSelecionada is not null && p is DocumentoFiscalDto);
        ImprimirUltimoReciboCommand = new RelayCommand(_ => ImprimirUltimoRecibo());
        EditarRascunhoCommand = new RelayCommand(EditarRascunho, p => p is ContaReceberListItemDto { PodeEditarItens: true });
        CancelarEdicaoCommand = new RelayCommand(_ => LimparFormulario());
        EditarItemCommand = new RelayCommand(EditarItem, p => p is DocumentoItemDto && ContaSelecionada?.PodeEditarItens == true);
        CancelarEdicaoItemCommand = new RelayCommand(_ => LimparFormularioItem());
        DuplicarDocumentoCommand = new AsyncRelayCommand(DuplicarDocumentoAsync);
        PesquisarFaturacaoCommand = new RelayCommand(_ => AplicarFiltrosFaturacao());
        LimparFiltrosFaturacaoCommand = new RelayCommand(_ => LimparFiltrosFaturacao());

        CriarCommand = new AsyncRelayCommand(_ => CriarAsync(), _ => !AGuardar);
        AprovarCommand = new AsyncRelayCommand(AprovarAsync);
        FaturarCommand = new AsyncRelayCommand(FaturarAsync);
        ReceberCommand = new AsyncRelayCommand(ReceberAsync);
        CancelarCommand = new AsyncRelayCommand(CancelarAsync);
        AtualizarCommand = new AsyncRelayCommand(_ => CarregarAsync(), _ => !ACarregar); AdicionarItemCommand=new AsyncRelayCommand(_=>AdicionarItemAsync()); RemoverItemCommand=new AsyncRelayCommand(p=>RemoverItemAsync(p)); NotaCreditoCommand=new AsyncRelayCommand(_=>EmitirNotaAsync("Credito")); NotaDebitoCommand=new AsyncRelayCommand(_=>EmitirNotaAsync("Debito"));

        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        MensagemErro = string.Empty;
        ACarregar = true;
        try
        {
            var empresa = await _service.ObterEmpresaAsync(_empresaId);
            if (empresa.IsSuccess) _empresaEmitente = empresa.Value;
            var produtos=await _service.ListarProdutosAsync(_empresaId); if(produtos.IsSuccess){ProdutosItens.Clear(); foreach(var x in produtos.Value??Array.Empty<ProdutoStockDto>()) ProdutosItens.Add(x);}
            var clientes = await _service.ListarClientesAsync(_empresaId);
            if (clientes.IsFailure) { MensagemErro = string.Join(" ", clientes.Errors); return; }
            Clientes.Clear();
            foreach (var cliente in clientes.Value ?? Array.Empty<ClienteOpcaoDto>()) Clientes.Add(cliente);

            var categorias = await _service.ListarCategoriasAsync(_empresaId);
            if (categorias.IsFailure) { MensagemErro = string.Join(" ", categorias.Errors); return; }
            Categorias.Clear();
            foreach (var categoria in categorias.Value ?? Array.Empty<CategoriaOpcaoDto>()) Categorias.Add(categoria);
            CategoriaSelecionada = Categorias.FirstOrDefault();

            var origens = await _service.ListarOrigensAsync(_empresaId);
            if (origens.IsFailure) { MensagemErro = string.Join(" ", origens.Errors); return; }
            Origens.Clear();
            foreach (var origem in origens.Value ?? Array.Empty<OpcaoOrigemDto>()) Origens.Add(origem);
            OrigemRecebimentoSelecionada = Origens.FirstOrDefault(o => o.Disponivel);

            await CarregarContasAsync();
        }
        finally
        {
            ACarregar = false;
        }
    }

    private async Task CarregarContasAsync()
    {
        var resultado = await _service.ListarAsync(_empresaId);
        if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }
        TodasContas.Clear(); TodasContas.AddRange(resultado.Value ?? Array.Empty<ContaReceberListItemDto>());
        AplicarFiltrosFaturacao();
    }

    private async Task CriarAsync()
    {
        LimparMensagens();

        decimal valor = 0;
        if (!ModoFaturacao && !decimal.TryParse(ValorTexto, out valor))
        {
            MensagemErro = "Indique um valor válido.";
            return;
        }
        if (!decimal.TryParse(DescontoGeralTexto, out var desconto) || desconto < 0 ||
            !decimal.TryParse(FreteTexto, out var frete) || frete < 0 ||
            !decimal.TryParse(OutrasDespesasTexto, out var outras) || outras < 0)
        {
            MensagemErro = "Desconto, frete e outras despesas devem ser valores válidos e não negativos.";
            return;
        }

        AGuardar = true;
        try
        {
            var resultado = EmEdicao
                ? await _service.AtualizarRascunhoAsync(new AtualizarFaturaRascunhoDto
                {
                    Id = _rascunhoIdEdicao!.Value, EmpresaId = _empresaId, Descricao = Descricao,
                    DataEmissao = DataEmissao, DataVencimento = DataVencimento,
                    ClienteId = ClienteSelecionado?.Id, CategoriaId = CategoriaSelecionada?.Id,
                    FormaPagamento = FormaPagamento, CentroCusto = CentroCusto,
                    DescontoGeral = desconto, Frete = frete, OutrasDespesas = outras, Observacoes = Observacoes
                })
                : await _service.CriarAsync(new NovaContaReceberDto
            {
                Descricao = Descricao,
                Valor = valor,
                DescontoGeral = desconto,
                Frete = frete,
                OutrasDespesas = outras,
                Observacoes = Observacoes,
                DataEmissao = DataEmissao,
                DataVencimento = DataVencimento,
                FormaPagamento = FormaPagamento,
                CentroCusto = CentroCusto,
                ClienteId = ClienteSelecionado?.Id,
                CategoriaId = CategoriaSelecionada?.Id,
                EmpresaId = _empresaId
            });

            if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }

            var editadoId = _rascunhoIdEdicao;
            LimparFormulario();
            await CarregarContasAsync();
            if (ModoFaturacao) ContaSelecionada = editadoId.HasValue ? Contas.FirstOrDefault(x => x.Id == editadoId.Value) : Contas.OrderByDescending(x => x.Id).FirstOrDefault();
            MensagemSucesso = resultado.Message ?? "Conta a receber registada com sucesso.";
        }
        finally
        {
            AGuardar = false;
        }
    }

    private async Task AprovarAsync(object? parametro)
    {
        LimparMensagens(); if (parametro is not ContaReceberListItemDto conta) return;
        var resultado = await _service.AprovarPropostaAsync(conta.Id);
        if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }
        await CarregarContasAsync(); MensagemSucesso = resultado.Message ?? "Proposta aprovada.";
    }

    private async Task FaturarAsync(object? parametro)
    {
        LimparMensagens(); if (parametro is not ContaReceberListItemDto conta) return;
        var resultado = await _service.FaturarAsync(conta.Id);
        if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }
        await CarregarContasAsync(); MensagemSucesso = resultado.Message ?? "Fatura emitida.";
    }

    private async Task ReceberAsync(object? parametro)
    {
        LimparMensagens();
        if (parametro is not ContaReceberListItemDto conta) return;
        if (OrigemRecebimentoSelecionada is null)
        {
            MensagemErro = "Selecione a origem do recebimento antes de continuar.";
            return;
        }

        var valor = conta.SaldoAberto;
        if (!string.IsNullOrWhiteSpace(ValorRecebimentoTexto) && (!decimal.TryParse(ValorRecebimentoTexto, out valor) || valor <= 0)) { MensagemErro = "Indique um valor de recebimento válido."; return; }
        var resultado = await _service.RegistarRecebimentoParcialAsync(
            conta.Id, valor, OrigemRecebimentoSelecionada.Tipo, OrigemRecebimentoSelecionada.Id, DataRecebimento);

        if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }
        var contaId = conta.Id;
        await CarregarContasAsync();
        ContaSelecionada = TodasContas.FirstOrDefault(x => x.Id == contaId);
        await CarregarDocumentosAsync();
        DocumentoFiscalSelecionado = UltimoRecibo;
        ValorRecebimentoTexto = string.Empty;
        MensagemSucesso = resultado.Message ?? $"Recebimento {conta.Codigo} confirmado.";
    }

    private async Task CancelarAsync(object? parametro)
    {
        LimparMensagens();
        if (parametro is not ContaReceberListItemDto conta) return;

        var resultado = await _service.CancelarAsync(conta.Id);
        if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }
        await CarregarContasAsync();
        MensagemSucesso = resultado.Message ?? $"Conta {conta.Codigo} cancelada.";
    }

    private async Task DuplicarDocumentoAsync(object? parametro)
    {
        LimparMensagens(); if (parametro is not ContaReceberListItemDto conta || !conta.PodeDuplicar) return;
        var resultado = await _service.DuplicarAsync(conta.Id, _empresaId);
        if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }
        PesquisaFaturacao = string.Empty; EstadoFaturacaoSelecionado = "Todos";
        DataInicialFaturacao = null; DataFinalFaturacao = null;
        await CarregarContasAsync(); ContaSelecionada = Contas.FirstOrDefault(x => x.Id == resultado.Value);
        await CarregarItensAsync(); MensagemSucesso = resultado.Message ?? "Novo rascunho criado.";
    }


    private async Task CarregarItensAsync()
    {
        Itens.Clear();
        if (ContaSelecionada is null) { NotificarTotalizadoresDocumento(); return; }
        var r = await _service.ListarItensAsync(ContaSelecionada.Id);
        if (r.IsFailure) { MensagemErro = string.Join(" ", r.Errors); NotificarTotalizadoresDocumento(); return; }
        foreach (var x in r.Value ?? Array.Empty<DocumentoItemDto>()) Itens.Add(x);
        NotificarTotalizadoresDocumento();
    }
    private async Task AdicionarItemAsync()
    {
        LimparMensagens();
        if (ContaSelecionada is null || ProdutoItemSelecionado is null)
        {
            MensagemErro = "Selecione uma fatura proforma em preparação e o produto ou serviço.";
            return;
        }
        if (!decimal.TryParse(QuantidadeItem, out var quantidade) || quantidade <= 0)
        {
            MensagemErro = "A quantidade deve ser maior do que zero.";
            return;
        }
        if (!decimal.TryParse(PrecoItem, out var preco) || preco < 0)
        {
            MensagemErro = "Indique um preço unitário válido.";
            return;
        }
        if (!decimal.TryParse(DescontoItem, out var desconto) || desconto < 0 || desconto > 100)
        {
            MensagemErro = "O desconto deve estar entre 0% e 100%.";
            return;
        }
        if (!decimal.TryParse(IvaItem, out var iva) || iva < 0 || iva > 100)
        {
            MensagemErro = "A taxa de IVA deve estar entre 0% e 100%.";
            return;
        }

        var documentoId = ContaSelecionada.Id;
        var resultado = ItemEmEdicao
            ? await _service.AtualizarItemAsync(new AtualizarDocumentoItemDto
            {
                Id = _itemIdEdicao!.Value, DocumentoId = documentoId, ProdutoId = ProdutoItemSelecionado.Id,
                Quantidade = quantidade, PrecoUnitario = preco, DescontoPercentual = desconto, IvaPercentual = iva
            })
            : await _service.AdicionarItemAsync(new NovoDocumentoItemDto
            {
                DocumentoId = documentoId, ProdutoId = ProdutoItemSelecionado.Id, Quantidade = quantidade,
                PrecoUnitario = preco, DescontoPercentual = desconto, IvaPercentual = iva
            });
        if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }

        LimparFormularioItem();
        await CarregarContasAsync();
        ContaSelecionada = TodasContas.FirstOrDefault(x => x.Id == documentoId);
        await CarregarItensAsync();
        MensagemSucesso = resultado.Message ?? "Item guardado.";
    }
    private async Task RemoverItemAsync(object? p){if(p is not DocumentoItemDto item||ContaSelecionada is null)return;var documentoId=ContaSelecionada.Id;var res=await _service.RemoverItemAsync(item.Id);if(res.IsFailure){MensagemErro=string.Join(" ",res.Errors);return;}await CarregarContasAsync();ContaSelecionada=TodasContas.FirstOrDefault(x=>x.Id==documentoId);await CarregarItensAsync();}

    private async Task CarregarDocumentosAsync(){DocumentosFiscais.Clear();if(ContaSelecionada is null){OnPropertyChanged(nameof(UltimoRecibo));OnPropertyChanged(nameof(PodeImprimirUltimoRecibo));return;}var r=await _service.ListarDocumentosAsync(ContaSelecionada.Id);if(r.IsFailure){MensagemErro=string.Join(" ",r.Errors);return;}foreach(var x in r.Value??Array.Empty<DocumentoFiscalDto>())DocumentosFiscais.Add(x);OnPropertyChanged(nameof(UltimoRecibo));OnPropertyChanged(nameof(PodeImprimirUltimoRecibo));}
    private async Task EmitirNotaAsync(string tipo){LimparMensagens();if(ContaSelecionada is null){MensagemErro="Selecione uma fatura.";return;}if(!decimal.TryParse(NotaValor,out var valor)||valor<=0){MensagemErro="Indique um valor válido para a nota.";return;}var r=await _service.EmitirNotaAsync(new NovaNotaFiscalDto{ContaReceberId=ContaSelecionada.Id,Tipo=tipo,Valor=valor,Motivo=NotaMotivo});if(r.IsFailure){MensagemErro=string.Join(" ",r.Errors);return;}NotaValor=string.Empty;NotaMotivo=string.Empty;await CarregarDocumentosAsync();MensagemSucesso=r.Message??"Documento fiscal emitido.";}

    private void LimparMensagens()
    {
        MensagemErro = string.Empty;
        MensagemSucesso = string.Empty;
    }

    private void ImprimirProforma()
    {
        if (ContaSelecionada is not null) FiscalDocumentPrinter.PrintProforma(_empresaEmitente, ContaSelecionada, Itens);
    }

    private void ImprimirDocumento(object? parametro)
    {
        if (ContaSelecionada is not null && parametro is DocumentoFiscalDto documento)
            FiscalDocumentPrinter.PrintFiscal(_empresaEmitente, documento, ContaSelecionada, Itens);
    }

    private void ImprimirUltimoRecibo()
    {
        if (ContaSelecionada is not null && UltimoRecibo is not null)
            FiscalDocumentPrinter.PrintFiscal(_empresaEmitente, UltimoRecibo, ContaSelecionada, Itens);
    }

    private void EditarRascunho(object? parametro)
    {
        if (parametro is not ContaReceberListItemDto conta || !conta.PodeEditarItens) return;
        _rascunhoIdEdicao = conta.Id;
        Descricao = conta.Descricao; DataEmissao = conta.DataEmissao; DataVencimento = conta.DataVencimento;
        ClienteSelecionado = Clientes.FirstOrDefault(x => x.Id == conta.ClienteId);
        CategoriaSelecionada = Categorias.FirstOrDefault(x => x.Id == conta.CategoriaId);
        FormaPagamento = conta.FormaPagamento ?? string.Empty; CentroCusto = conta.CentroCusto ?? string.Empty;
        DescontoGeralTexto = conta.DescontoGeral.ToString(); FreteTexto = conta.Frete.ToString(); OutrasDespesasTexto = conta.OutrasDespesas.ToString();
        Observacoes = conta.Observacoes ?? string.Empty;
        OnPropertyChanged(nameof(EmEdicao)); OnPropertyChanged(nameof(TextoAcaoDocumento));
    }

    private void LimparFormulario()
    {
        _rascunhoIdEdicao = null; Descricao = string.Empty; ValorTexto = string.Empty;
        DescontoGeralTexto = "0"; FreteTexto = "0"; OutrasDespesasTexto = "0"; Observacoes = string.Empty;
        FormaPagamento = string.Empty; CentroCusto = string.Empty; ClienteSelecionado = null;
        OnPropertyChanged(nameof(EmEdicao)); OnPropertyChanged(nameof(TextoAcaoDocumento));
    }

    private void EditarItem(object? parametro)
    {
        if (parametro is not DocumentoItemDto item || ContaSelecionada?.PodeEditarItens != true) return;
        _itemIdEdicao = item.Id; ProdutoItemSelecionado = ProdutosItens.FirstOrDefault(x => x.Id == item.ProdutoId);
        QuantidadeItem = item.Quantidade.ToString(); PrecoItem = item.PrecoUnitario.ToString();
        DescontoItem = item.DescontoPercentual.ToString(); IvaItem = item.IvaPercentual.ToString();
        OnPropertyChanged(nameof(ItemEmEdicao)); OnPropertyChanged(nameof(TextoAcaoItem));
    }

    private void LimparFormularioItem()
    {
        _itemIdEdicao = null; ProdutoItemSelecionado = null; QuantidadeItem = "1"; PrecoItem = string.Empty; DescontoItem = "0"; IvaItem = "0";
        OnPropertyChanged(nameof(ItemEmEdicao)); OnPropertyChanged(nameof(TextoAcaoItem));
    }

    private void AplicarFiltrosFaturacao()
    {
        IEnumerable<ContaReceberListItemDto> query = TodasContas;
        if (ModoFaturacao && !string.IsNullOrWhiteSpace(PesquisaFaturacao))
        {
            var termo = PesquisaFaturacao.Trim();
            query = query.Where(x => (x.NumeroProposta?.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false)
                || (x.NumeroFatura?.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false)
                || x.Descricao.Contains(termo, StringComparison.OrdinalIgnoreCase)
                || (x.ClienteNome?.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        if (ModoFaturacao && EstadoFaturacaoSelecionado != "Todos") query = query.Where(CorrespondeEstadoFaturacao);
        if (ModoFaturacao && DataInicialFaturacao.HasValue) query = query.Where(x => x.DataEmissao.Date >= DataInicialFaturacao.Value.Date);
        if (ModoFaturacao && DataFinalFaturacao.HasValue) query = query.Where(x => x.DataEmissao.Date <= DataFinalFaturacao.Value.Date);
        Contas.Clear(); foreach (var conta in query.OrderByDescending(x => x.Id)) Contas.Add(conta);
        NotificarResumoFaturacao();
    }

    private bool CorrespondeEstadoFaturacao(ContaReceberListItemDto x) => EstadoFaturacaoSelecionado switch
    {
        "Rascunho" => x.ComercialEstado == "Proposta",
        "Proforma validada" => x.ComercialEstado == "Aprovada",
        "Fatura definitiva" => x.ComercialEstado == "Faturada" && x.ValorLiquidado == 0
            && x.EstadoExibicao != "Recebido" && x.EstadoExibicao != "Cancelado",
        "Parcialmente paga" => x.ComercialEstado == "Faturada" && x.ValorLiquidado > 0 && x.SaldoAberto > 0,
        "Paga" => x.EstadoExibicao == "Recebido",
        "Vencida" => x.DiasAtraso > 0,
        "Cancelada" => x.EstadoExibicao == "Cancelado",
        _ => true
    };

    private void LimparFiltrosFaturacao()
    {
        PesquisaFaturacao = string.Empty; EstadoFaturacaoSelecionado = "Todos";
        DataInicialFaturacao = null; DataFinalFaturacao = null; AplicarFiltrosFaturacao();
    }

    private void NotificarResumoFaturacao()
    {
        OnPropertyChanged(nameof(DocumentosEmAberto));
        OnPropertyChanged(nameof(ValorFaturado));
        OnPropertyChanged(nameof(ValorRecebido));
        OnPropertyChanged(nameof(SaldoPendente));
        OnPropertyChanged(nameof(MoedaFaturacao));
        OnPropertyChanged(nameof(ValorFaturadoTexto));
        OnPropertyChanged(nameof(ValorRecebidoTexto));
        OnPropertyChanged(nameof(SaldoPendenteTexto));
    }

    private void NotificarTotalizadoresDocumento()
    {
        OnPropertyChanged(nameof(QuantidadeItensDocumento));
        OnPropertyChanged(nameof(QuantidadeItensDocumentoTexto));
        OnPropertyChanged(nameof(BaseTributavelDocumento));
        OnPropertyChanged(nameof(IvaDocumento));
        OnPropertyChanged(nameof(DescontoDocumento));
        OnPropertyChanged(nameof(EncargosDocumento));
        OnPropertyChanged(nameof(TotalDocumento));
        OnPropertyChanged(nameof(BaseTributavelDocumentoTexto));
        OnPropertyChanged(nameof(IvaDocumentoTexto));
        OnPropertyChanged(nameof(DescontoDocumentoTexto));
        OnPropertyChanged(nameof(EncargosDocumentoTexto));
        OnPropertyChanged(nameof(TotalDocumentoTexto));
    }
}
