using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class StockViewModel : ViewModelBase
{
    private readonly IStockService _service;
    private readonly int _empresaId;
    private readonly List<ProdutoStockDto> _todosProdutos = new();
    private int _produtoEdicaoId;
    private string _codigo = "", _nome = "", _categoria = "", _unidade = "UN", _minimo = "0", _precoVenda = "0", _localizacao = "";
    private bool _controlaStock = true;
    private string _quantidade = "", _custo = "", _documento = "", _observacao = "", _pesquisa = "", _mensagem = "";
    private bool _apenasAlertas, _aCarregar;
    private ProdutoStockDto? _produto;
    private TipoMovimentoStock _tipo = TipoMovimentoStock.Entrada;

    public ObservableCollection<ProdutoStockDto> Produtos { get; } = new();
    public ObservableCollection<MovimentoStockDto> Movimentos { get; } = new();
    public ObservableCollection<ProdutoRentabilidadeDto> Rentabilidade { get; } = new();
    public IReadOnlyList<TipoMovimentoStock> Tipos { get; } = Enum.GetValues<TipoMovimentoStock>();

    public string Codigo { get => _codigo; set => SetProperty(ref _codigo, value); }
    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string Categoria { get => _categoria; set => SetProperty(ref _categoria, value); }
    public string Unidade { get => _unidade; set => SetProperty(ref _unidade, value); }
    public string Minimo { get => _minimo; set => SetProperty(ref _minimo, value); }
    public string PrecoVenda { get => _precoVenda; set => SetProperty(ref _precoVenda, value); }
    public bool ControlaStock { get => _controlaStock; set { if (SetProperty(ref _controlaStock, value)) { OnPropertyChanged(nameof(TipoItemFormulario)); OnPropertyChanged(nameof(TituloFormulario)); OnPropertyChanged(nameof(TextoGuardar)); } } }
    public string TipoItemFormulario => ControlaStock ? "Produto" : "Serviço";
    public string Localizacao { get => _localizacao; set => SetProperty(ref _localizacao, value); }
    public string Quantidade { get => _quantidade; set => SetProperty(ref _quantidade, value); }
    public string Custo { get => _custo; set => SetProperty(ref _custo, value); }
    public string Documento { get => _documento; set => SetProperty(ref _documento, value); }
    public string Observacao { get => _observacao; set => SetProperty(ref _observacao, value); }
    public string Pesquisa { get => _pesquisa; set { if (SetProperty(ref _pesquisa, value)) AplicarFiltro(); } }
    public bool ApenasAlertas { get => _apenasAlertas; set { if (SetProperty(ref _apenasAlertas, value)) AplicarFiltro(); } }
    public bool ACarregar { get => _aCarregar; private set => SetProperty(ref _aCarregar, value); }
    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public ProdutoStockDto? ProdutoSelecionado { get => _produto; set => SetProperty(ref _produto, value); }
    public TipoMovimentoStock Tipo { get => _tipo; set => SetProperty(ref _tipo, value); }
    public bool EmEdicao => _produtoEdicaoId > 0;
    public string TituloFormulario => EmEdicao ? $"Editar {TipoItemFormulario.ToLowerInvariant()}" : $"Novo {TipoItemFormulario.ToLowerInvariant()}";
    public string TextoGuardar => EmEdicao ? "Guardar alterações" : $"Criar {TipoItemFormulario.ToLowerInvariant()}";
    public decimal ValorTotalStock => _todosProdutos.Sum(x => x.ValorStock);
    public int TotalProdutos => _todosProdutos.Count;
    public int Alertas => _todosProdutos.Count(x => x.AbaixoMinimo);
    public int SemStock => _todosProdutos.Count(x => x.ControlaStock && x.StockAtual <= 0);

    private decimal _receitaLiquida, _custoVendido, _margemBruta, _margemPercentual;
    private int _margensNegativas;
    public decimal ReceitaLiquida { get => _receitaLiquida; private set => SetProperty(ref _receitaLiquida, value); }
    public decimal CustoVendido { get => _custoVendido; private set => SetProperty(ref _custoVendido, value); }
    public decimal MargemBruta { get => _margemBruta; private set => SetProperty(ref _margemBruta, value); }
    public decimal MargemPercentual { get => _margemPercentual; private set => SetProperty(ref _margemPercentual, value); }
    public int MargensNegativas { get => _margensNegativas; private set => SetProperty(ref _margensNegativas, value); }

    public ICommand GuardarProdutoCommand { get; }
    public ICommand NovoProdutoCommand { get; }
    public ICommand EditarProdutoCommand { get; }
    public ICommand ArquivarProdutoCommand { get; }
    public ICommand MovimentarCommand { get; }
    public ICommand AtualizarCommand { get; }
    public ICommand ExportarProdutosCommand { get; }
    public ICommand ExportarMovimentosCommand { get; }

    public StockViewModel(IStockService service, int empresaId)
    {
        _service = service; _empresaId = empresaId;
        GuardarProdutoCommand = new AsyncRelayCommand(_ => GuardarProdutoAsync());
        NovoProdutoCommand = new RelayCommand(_ => LimparProduto());
        EditarProdutoCommand = new RelayCommand(_ => CarregarProdutoParaEdicao(), _ => ProdutoSelecionado is not null);
        ArquivarProdutoCommand = new AsyncRelayCommand(_ => ArquivarProdutoAsync(), _ => ProdutoSelecionado is not null);
        MovimentarCommand = new AsyncRelayCommand(_ => MovimentarAsync());
        AtualizarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        ExportarProdutosCommand = new RelayCommand(_ => ExportarProdutos());
        ExportarMovimentosCommand = new RelayCommand(_ => ExportarMovimentos());
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        ACarregar = true; Mensagem = string.Empty;
        try
        {
            _todosProdutos.Clear(); _todosProdutos.AddRange(await _service.ListarProdutosAsync(_empresaId)); AplicarFiltro();
            Movimentos.Clear(); foreach (var item in await _service.ListarMovimentosAsync(_empresaId)) Movimentos.Add(item);
            Rentabilidade.Clear(); foreach (var item in await _service.ListarRentabilidadeProdutosAsync(_empresaId)) Rentabilidade.Add(item);
            var resumo = await _service.ObterResumoCustosMargensAsync(_empresaId);
            ReceitaLiquida = resumo.ReceitaLiquida; CustoVendido = resumo.CustoProdutosVendidos; MargemBruta = resumo.MargemBruta;
            MargemPercentual = resumo.MargemPercentual; MargensNegativas = resumo.ProdutosComMargemNegativa; AtualizarIndicadores();
        }
        catch (Exception ex) { Mensagem = ex.Message; }
        finally { ACarregar = false; }
    }

    private void AplicarFiltro()
    {
        var termo = Pesquisa.Trim();
        var lista = _todosProdutos.Where(x => (!ApenasAlertas || x.AbaixoMinimo) && (termo.Length == 0 ||
            x.Codigo.Contains(termo, StringComparison.CurrentCultureIgnoreCase) || x.Nome.Contains(termo, StringComparison.CurrentCultureIgnoreCase) ||
            x.Categoria.Contains(termo, StringComparison.CurrentCultureIgnoreCase) || x.Localizacao.Contains(termo, StringComparison.CurrentCultureIgnoreCase)));
        Produtos.Clear(); foreach (var item in lista) Produtos.Add(item);
    }

    private async Task GuardarProdutoAsync()
    {
        try
        {
            ValidarProduto();
            var dto = new NovoProdutoDto { EmpresaId = _empresaId, Codigo = Codigo, Nome = Nome, Categoria = Categoria, Unidade = Unidade, StockMinimo = ControlaStock ? Decimal(Minimo) : 0, PrecoVenda = Decimal(PrecoVenda), ControlaStock = ControlaStock, Localizacao = ControlaStock ? Localizacao : null };
            var edicao = EmEdicao;
            if (edicao) await _service.AtualizarProdutoAsync(_produtoEdicaoId, dto); else await _service.CriarProdutoAsync(dto);
            LimparProduto(false); await CarregarAsync(); Mensagem = edicao ? "Produto atualizado com sucesso." : "Produto criado com sucesso.";
        }
        catch (Exception ex) { Mensagem = ex.Message; }
    }

    private void CarregarProdutoParaEdicao()
    {
        if (ProdutoSelecionado is null) return;
        _produtoEdicaoId = ProdutoSelecionado.Id; Codigo = ProdutoSelecionado.Codigo; Nome = ProdutoSelecionado.Nome; Categoria = ProdutoSelecionado.Categoria;
        Unidade = ProdutoSelecionado.Unidade; Minimo = ProdutoSelecionado.StockMinimo.ToString("N3"); PrecoVenda = ProdutoSelecionado.PrecoVenda.ToString("N2"); ControlaStock = ProdutoSelecionado.ControlaStock; Localizacao = ProdutoSelecionado.Localizacao;
        OnPropertyChanged(nameof(EmEdicao)); OnPropertyChanged(nameof(TituloFormulario)); OnPropertyChanged(nameof(TextoGuardar)); Mensagem = "Produto carregado para edição.";
    }

    private async Task ArquivarProdutoAsync()
    {
        if (ProdutoSelecionado is null) return;
        try { await _service.ArquivarProdutoAsync(_empresaId, ProdutoSelecionado.Id); LimparProduto(false); await CarregarAsync(); Mensagem = "Produto arquivado com sucesso."; }
        catch (Exception ex) { Mensagem = ex.Message; }
    }

    private async Task MovimentarAsync()
    {
        try
        {
            if (ProdutoSelecionado is null) throw new InvalidOperationException("Selecione um produto.");
            var quantidade = Decimal(Quantidade); if (quantidade <= 0) throw new InvalidOperationException("A quantidade deve ser maior que zero.");
            await _service.MovimentarAsync(new NovoMovimentoStockDto { EmpresaId = _empresaId, ProdutoId = ProdutoSelecionado.Id, Tipo = Tipo, Quantidade = quantidade, CustoUnitario = Decimal(Custo), DocumentoReferencia = Documento, Observacao = Observacao });
            Quantidade = Custo = Documento = Observacao = string.Empty; await CarregarAsync(); Mensagem = "Movimento registado com sucesso.";
        }
        catch (Exception ex) { Mensagem = ex.Message; }
    }

    private void ValidarProduto()
    {
        if (string.IsNullOrWhiteSpace(Codigo)) throw new InvalidOperationException("O código é obrigatório.");
        if (string.IsNullOrWhiteSpace(Nome)) throw new InvalidOperationException("O nome é obrigatório.");
        if (ControlaStock && Decimal(Minimo) < 0) throw new InvalidOperationException("O stock mínimo não pode ser negativo.");
        if (Decimal(PrecoVenda) < 0) throw new InvalidOperationException("O preço de venda não pode ser negativo.");
    }

    private void LimparProduto(bool limparMensagem = true)
    {
        _produtoEdicaoId = 0; Codigo = Nome = Categoria = Localizacao = string.Empty; Unidade = "UN"; Minimo = PrecoVenda = "0"; ControlaStock = true; ProdutoSelecionado = null;
        OnPropertyChanged(nameof(EmEdicao)); OnPropertyChanged(nameof(TituloFormulario)); OnPropertyChanged(nameof(TextoGuardar)); if (limparMensagem) Mensagem = string.Empty;
    }

    private void ExportarProdutos()
    {
        if (ExportadorCsv.Exportar("stocks-produtos.csv", ["Código", "Produto", "Categoria", "Unidade", "Stock", "Mínimo", "Custo médio", "Valor", "Localização"], Produtos.Select(x => (IReadOnlyList<string>)[x.Codigo, x.Nome, x.Categoria, x.Unidade, x.StockAtual.ToString("N3"), x.StockMinimo.ToString("N3"), x.CustoMedio.ToString("N2"), x.ValorStock.ToString("N2"), x.Localizacao]))) Mensagem = "Produtos exportados com sucesso.";
    }

    private void ExportarMovimentos()
    {
        if (ExportadorCsv.Exportar("stocks-movimentos.csv", ["Data", "Produto", "Tipo", "Quantidade", "Custo", "Saldo", "Documento"], Movimentos.Select(x => (IReadOnlyList<string>)[x.Data.ToString("dd/MM/yyyy HH:mm"), x.Produto, x.Tipo, x.Quantidade.ToString("N3"), x.CustoUnitario.ToString("N2"), x.SaldoApos.ToString("N3"), x.DocumentoReferencia]))) Mensagem = "Movimentos exportados com sucesso.";
    }

    private void AtualizarIndicadores() { OnPropertyChanged(nameof(ValorTotalStock)); OnPropertyChanged(nameof(TotalProdutos)); OnPropertyChanged(nameof(Alertas)); OnPropertyChanged(nameof(SemStock)); }
    private static decimal Decimal(string texto) => decimal.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, out var valor) || decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out valor) ? valor : 0;
}
