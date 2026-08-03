using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Application.Receivables;
using FinancePro.Core.DTOs;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class ReceitasViewModel : ViewModelBase
{
    private readonly ReceivablesApplicationService _service;
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
    private DateTime _dataRecebimento = DateTime.Today;
    private OpcaoOrigemDto? _origemRecebimentoSelecionada;
    private string _pesquisa = string.Empty;
    private string _estadoSelecionado = "Todos";
    private decimal _totalAberto;
    private decimal _totalVencido;
    private decimal _totalHoje;
    private int _quantidadeAberta;

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
    public DateTime DataRecebimento { get => _dataRecebimento; set => SetProperty(ref _dataRecebimento, value); }
    public OpcaoOrigemDto? OrigemRecebimentoSelecionada { get => _origemRecebimentoSelecionada; set => SetProperty(ref _origemRecebimentoSelecionada, value); }
    public string Pesquisa { get => _pesquisa; set => SetProperty(ref _pesquisa, value); }
    public string EstadoSelecionado { get => _estadoSelecionado; set => SetProperty(ref _estadoSelecionado, value); }
    public decimal TotalAberto { get => _totalAberto; private set => SetProperty(ref _totalAberto, value); }
    public decimal TotalVencido { get => _totalVencido; private set => SetProperty(ref _totalVencido, value); }
    public decimal TotalHoje { get => _totalHoje; private set => SetProperty(ref _totalHoje, value); }
    public int QuantidadeAberta { get => _quantidadeAberta; private set => SetProperty(ref _quantidadeAberta, value); }

    public ObservableCollection<ClienteOpcaoDto> Clientes { get; } = new();
    public ObservableCollection<CategoriaOpcaoDto> Categorias { get; } = new();
    public ObservableCollection<OpcaoOrigemDto> Origens { get; } = new();
    public ObservableCollection<ContaReceberListItemDto> Contas { get; } = new();
    public ObservableCollection<string> Estados { get; } = new(new[] { "Todos", "Pendente", "Atrasado", "Recebido", "Cancelado" });

    public ICommand CriarCommand { get; }
    public ICommand ReceberCommand { get; }
    public ICommand CancelarCommand { get; }
    public ICommand AtualizarCommand { get; }
    public ICommand FiltrarCommand { get; }

    public ReceitasViewModel(ReceivablesApplicationService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;
        CriarCommand = new AsyncRelayCommand(_ => CriarAsync(), _ => !AGuardar);
        ReceberCommand = new AsyncRelayCommand(ReceberAsync);
        CancelarCommand = new AsyncRelayCommand(CancelarAsync);
        AtualizarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        FiltrarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        MensagemErro = string.Empty;
        var result = await _service.LoadAsync(new ReceivablesQuery(_empresaId, Pesquisa, EstadoSelecionado));
        if (result.IsFailure || result.Value is null)
        {
            MensagemErro = result.Message ?? result.Errors.FirstOrDefault() ?? "Não foi possível carregar as contas a receber.";
            return;
        }

        Replace(Clientes, result.Value.Customers);
        Replace(Categorias, result.Value.Categories);
        Replace(Origens, result.Value.Origins);
        Replace(Contas, result.Value.Items);
        CategoriaSelecionada ??= Categorias.FirstOrDefault();
        OrigemRecebimentoSelecionada ??= Origens.FirstOrDefault(x => x.Disponivel);
        TotalAberto = result.Value.Summary.OpenAmount;
        TotalVencido = result.Value.Summary.OverdueAmount;
        TotalHoje = result.Value.Summary.DueTodayAmount;
        QuantidadeAberta = result.Value.Summary.OpenCount;
    }

    private async Task CriarAsync()
    {
        MensagemErro = MensagemSucesso = string.Empty;
        if (!decimal.TryParse(ValorTexto, out var valor)) { MensagemErro = "Indique um valor válido."; return; }
        AGuardar = true;
        try
        {
            var result = await _service.CreateAsync(new NovaContaReceberDto
            {
                Descricao = Descricao, Valor = valor, DataEmissao = DataEmissao, DataVencimento = DataVencimento,
                FormaPagamento = FormaPagamento, CentroCusto = CentroCusto, ClienteId = ClienteSelecionado?.Id,
                CategoriaId = CategoriaSelecionada?.Id, EmpresaId = _empresaId
            });
            if (result.IsFailure) { MensagemErro = result.Message ?? string.Join("\n", result.Errors); return; }
            Descricao = ValorTexto = string.Empty;
            MensagemSucesso = result.Message ?? "Conta a receber registada.";
            await CarregarAsync();
        }
        finally { AGuardar = false; }
    }

    private async Task ReceberAsync(object? parameter)
    {
        if (parameter is not ContaReceberListItemDto conta) return;
        var result = await _service.ReceiveAsync(conta.Id, OrigemRecebimentoSelecionada, DataRecebimento);
        if (result.IsFailure) { MensagemErro = result.Message ?? string.Join("\n", result.Errors); return; }
        MensagemSucesso = result.Message ?? "Recebimento confirmado.";
        await CarregarAsync();
    }

    private async Task CancelarAsync(object? parameter)
    {
        if (parameter is not ContaReceberListItemDto conta) return;
        var result = await _service.CancelAsync(conta.Id);
        if (result.IsFailure) { MensagemErro = result.Message ?? string.Join("\n", result.Errors); return; }
        MensagemSucesso = result.Message ?? "Conta cancelada.";
        await CarregarAsync();
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source) target.Add(item);
    }
}
