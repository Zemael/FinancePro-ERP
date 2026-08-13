using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Application.Revenue;
using FinancePro.Core.DTOs;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class ReceitasViewModel : ViewModelBase
{
    private readonly RevenueApplicationService _service;
    private readonly int _empresaId;

    private string _descricao = string.Empty;
    private string _valorTexto = string.Empty;
    private DateTime _dataEmissao = DateTime.Today;
    private DateTime _dataVencimento = DateTime.Today;
    private ClienteOpcaoDto? _clienteSelecionado;
    private CategoriaOpcaoDto? _categoriaSelecionada;
    private string _formaPagamento = string.Empty;
    private string _centroCusto = string.Empty;
    private string _mensagemErro = string.Empty;
    private string _mensagemSucesso = string.Empty;
    private bool _aGuardar;
    private bool _aCarregar;
    private DateTime _dataRecebimento = DateTime.Today;
    private string _valorRecebimentoTexto = string.Empty;
    private OpcaoOrigemDto? _origemRecebimentoSelecionada;
    private ContaReceberListItemDto? _contaSelecionada; private ProdutoStockDto? _produtoItemSelecionado; private string _quantidadeItem="1", _precoItem="", _descontoItem="0", _ivaItem="0";

    public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
    public string ValorTexto { get => _valorTexto; set => SetProperty(ref _valorTexto, value); }
    public DateTime DataEmissao { get => _dataEmissao; set => SetProperty(ref _dataEmissao, value); }
    public DateTime DataVencimento { get => _dataVencimento; set => SetProperty(ref _dataVencimento, value); }
    public ClienteOpcaoDto? ClienteSelecionado { get => _clienteSelecionado; set => SetProperty(ref _clienteSelecionado, value); }
    public CategoriaOpcaoDto? CategoriaSelecionada { get => _categoriaSelecionada; set => SetProperty(ref _categoriaSelecionada, value); }
    public string FormaPagamento { get => _formaPagamento; set => SetProperty(ref _formaPagamento, value); }
    public string CentroCusto { get => _centroCusto; set => SetProperty(ref _centroCusto, value); }
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
    public ContaReceberListItemDto? ContaSelecionada {get=>_contaSelecionada;set{if(SetProperty(ref _contaSelecionada,value)){_=CarregarItensAsync();_=CarregarDocumentosAsync();}}} public ProdutoStockDto? ProdutoItemSelecionado{get=>_produtoItemSelecionado;set=>SetProperty(ref _produtoItemSelecionado,value);} public string QuantidadeItem{get=>_quantidadeItem;set=>SetProperty(ref _quantidadeItem,value);} public string PrecoItem{get=>_precoItem;set=>SetProperty(ref _precoItem,value);} public string DescontoItem{get=>_descontoItem;set=>SetProperty(ref _descontoItem,value);} public string IvaItem{get=>_ivaItem;set=>SetProperty(ref _ivaItem,value);}

    public ObservableCollection<ClienteOpcaoDto> Clientes { get; } = new();
    public ObservableCollection<CategoriaOpcaoDto> Categorias { get; } = new();
    public ObservableCollection<OpcaoOrigemDto> Origens { get; } = new();
    public ObservableCollection<ContaReceberListItemDto> Contas { get; } = new(); public ObservableCollection<ProdutoStockDto> ProdutosItens{get;}=new(); public ObservableCollection<DocumentoItemDto> Itens{get;}=new(); public ObservableCollection<DocumentoFiscalDto> DocumentosFiscais{get;}=new();

    public ICommand CriarCommand { get; }
    public ICommand AprovarCommand { get; }
    public ICommand FaturarCommand { get; }
    public ICommand ReceberCommand { get; }
    public ICommand CancelarCommand { get; }
    public ICommand AtualizarCommand { get; } public ICommand AdicionarItemCommand{get;} public ICommand RemoverItemCommand{get;} public ICommand NotaCreditoCommand{get;} public ICommand NotaDebitoCommand{get;}

    public ReceitasViewModel(RevenueApplicationService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;

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
        Contas.Clear();
        foreach (var conta in resultado.Value ?? Array.Empty<ContaReceberListItemDto>()) Contas.Add(conta);
    }

    private async Task CriarAsync()
    {
        LimparMensagens();

        if (!decimal.TryParse(ValorTexto, out var valor))
        {
            MensagemErro = "Indique um valor válido.";
            return;
        }

        AGuardar = true;
        try
        {
            var resultado = await _service.CriarAsync(new NovaContaReceberDto
            {
                Descricao = Descricao,
                Valor = valor,
                DataEmissao = DataEmissao,
                DataVencimento = DataVencimento,
                FormaPagamento = FormaPagamento,
                CentroCusto = CentroCusto,
                ClienteId = ClienteSelecionado?.Id,
                CategoriaId = CategoriaSelecionada?.Id,
                EmpresaId = _empresaId
            });

            if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }

            Descricao = string.Empty;
            ValorTexto = string.Empty;
            await CarregarContasAsync();
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
        await CarregarContasAsync();
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


    private async Task CarregarItensAsync(){Itens.Clear();if(ContaSelecionada is null)return;var r=await _service.ListarItensAsync(ContaSelecionada.Id);if(r.IsFailure){MensagemErro=string.Join(" ",r.Errors);return;}foreach(var x in r.Value??Array.Empty<DocumentoItemDto>())Itens.Add(x);}
    private async Task AdicionarItemAsync(){LimparMensagens();if(ContaSelecionada is null||ProdutoItemSelecionado is null){MensagemErro="Selecione a proposta e o produto.";return;}if(!decimal.TryParse(QuantidadeItem,out var q)||!decimal.TryParse(PrecoItem,out var p)||!decimal.TryParse(DescontoItem,out var d)||!decimal.TryParse(IvaItem,out var i)){MensagemErro="Valores do item inválidos.";return;}var res=await _service.AdicionarItemAsync(new NovoDocumentoItemDto{DocumentoId=ContaSelecionada.Id,ProdutoId=ProdutoItemSelecionado.Id,Quantidade=q,PrecoUnitario=p,DescontoPercentual=d,IvaPercentual=i});if(res.IsFailure){MensagemErro=string.Join(" ",res.Errors);return;}await CarregarItensAsync();await CarregarContasAsync();MensagemSucesso=res.Message??"Item adicionado.";}
    private async Task RemoverItemAsync(object? p){if(p is not DocumentoItemDto item)return;var res=await _service.RemoverItemAsync(item.Id);if(res.IsFailure){MensagemErro=string.Join(" ",res.Errors);return;}await CarregarItensAsync();await CarregarContasAsync();}

    private async Task CarregarDocumentosAsync(){DocumentosFiscais.Clear();if(ContaSelecionada is null)return;var r=await _service.ListarDocumentosAsync(ContaSelecionada.Id);if(r.IsFailure){MensagemErro=string.Join(" ",r.Errors);return;}foreach(var x in r.Value??Array.Empty<DocumentoFiscalDto>())DocumentosFiscais.Add(x);}
    private async Task EmitirNotaAsync(string tipo){LimparMensagens();if(ContaSelecionada is null){MensagemErro="Selecione uma fatura.";return;}if(!decimal.TryParse(NotaValor,out var valor)||valor<=0){MensagemErro="Indique um valor válido para a nota.";return;}var r=await _service.EmitirNotaAsync(new NovaNotaFiscalDto{ContaReceberId=ContaSelecionada.Id,Tipo=tipo,Valor=valor,Motivo=NotaMotivo});if(r.IsFailure){MensagemErro=string.Join(" ",r.Errors);return;}NotaValor=string.Empty;NotaMotivo=string.Empty;await CarregarDocumentosAsync();MensagemSucesso=r.Message??"Documento fiscal emitido.";}

    private void LimparMensagens()
    {
        MensagemErro = string.Empty;
        MensagemSucesso = string.Empty;
    }
}
