using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using FinancePro.Application.Payables;
using FinancePro.Core.DTOs;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class DespesasViewModel : ViewModelBase
{
    private readonly PayablesApplicationService _service;
    private readonly int _empresaId;

    private string _descricao = string.Empty;
    private string _valorTexto = string.Empty;
    private DateTime _dataEmissao = DateTime.Today;
    private DateTime _dataVencimento = DateTime.Today;
    private FornecedorOpcaoDto? _fornecedorSelecionado;
    private CategoriaOpcaoDto? _categoriaSelecionada;
    private string _formaPagamento = string.Empty;
    private string _centroCusto = string.Empty;
    private string _mensagemErro = string.Empty;
    private bool _aGuardar;
    private OpcaoOrigemDto? _origemPagamentoSelecionada;
    private DateTime _dataPagamento = DateTime.Today;
    private bool _aProcessarPagamento;
    private bool _aCarregar;

    public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
    public string ValorTexto { get => _valorTexto; set => SetProperty(ref _valorTexto, value); }
    public DateTime DataEmissao { get => _dataEmissao; set => SetProperty(ref _dataEmissao, value); }
    public DateTime DataVencimento { get => _dataVencimento; set => SetProperty(ref _dataVencimento, value); }
    public FornecedorOpcaoDto? FornecedorSelecionado { get => _fornecedorSelecionado; set => SetProperty(ref _fornecedorSelecionado, value); }
    public CategoriaOpcaoDto? CategoriaSelecionada { get => _categoriaSelecionada; set => SetProperty(ref _categoriaSelecionada, value); }
    public string FormaPagamento { get => _formaPagamento; set => SetProperty(ref _formaPagamento, value); }
    public string CentroCusto { get => _centroCusto; set => SetProperty(ref _centroCusto, value); }
    public string MensagemErro { get => _mensagemErro; set => SetProperty(ref _mensagemErro, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }
    public DateTime DataPagamento { get => _dataPagamento; set => SetProperty(ref _dataPagamento, value); }
    public bool AProcessarPagamento { get => _aProcessarPagamento; set => SetProperty(ref _aProcessarPagamento, value); }
    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }
    public OpcaoOrigemDto? OrigemPagamentoSelecionada { get => _origemPagamentoSelecionada; set => SetProperty(ref _origemPagamentoSelecionada, value); }

    public ObservableCollection<FornecedorOpcaoDto> Fornecedores { get; } = new();
    public ObservableCollection<CategoriaOpcaoDto> Categorias { get; } = new();
    public ObservableCollection<OpcaoOrigemDto> Origens { get; } = new();
    public ObservableCollection<ContaPagarListItemDto> Contas { get; } = new();

    public ICommand CriarCommand { get; }
    public ICommand PagarCommand { get; }
    public ICommand CancelarCommand { get; }
    public ICommand AtualizarCommand { get; }

    public DespesasViewModel(PayablesApplicationService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;

        CriarCommand = new AsyncRelayCommand(_ => CriarAsync(), _ => !AGuardar);
        PagarCommand = new AsyncRelayCommand(PagarAsync);
        CancelarCommand = new AsyncRelayCommand(CancelarAsync);
        AtualizarCommand = new AsyncRelayCommand(_ => CarregarAsync(), _ => !ACarregar);

        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        ACarregar = true;
        MensagemErro = string.Empty;
        try
        {
            var result = await _service.LoadAsync(new PayablesQuery(_empresaId));
            if (result.IsFailure || result.Value is null)
            {
                MensagemErro = BuildMessage(result.Message, result.Errors);
                return;
            }

            Replace(Fornecedores, result.Value.Suppliers);
            Replace(Categorias, result.Value.Categories);
            Replace(Origens, result.Value.Origins);
            Replace(Contas, result.Value.Items);

            CategoriaSelecionada ??= Categorias.FirstOrDefault();
            OrigemPagamentoSelecionada ??= Origens.FirstOrDefault();
        }
        finally
        {
            ACarregar = false;
        }
    }

    private async Task CriarAsync()
    {
        MensagemErro = string.Empty;
        if (!decimal.TryParse(ValorTexto, NumberStyles.Number, CultureInfo.CurrentCulture, out var valor) || valor <= 0)
        {
            MensagemErro = "Indique um valor válido.";
            return;
        }

        AGuardar = true;
        try
        {
            var result = await _service.CreateAsync(new NovaContaPagarDto
            {
                Descricao = Descricao,
                Valor = valor,
                DataEmissao = DataEmissao,
                DataVencimento = DataVencimento,
                FormaPagamento = FormaPagamento,
                CentroCusto = CentroCusto,
                FornecedorId = FornecedorSelecionado?.Id,
                CategoriaId = CategoriaSelecionada?.Id,
                EmpresaId = _empresaId
            });

            if (result.IsFailure)
            {
                MensagemErro = BuildMessage(result.Message, result.Errors);
                return;
            }

            Descricao = string.Empty;
            ValorTexto = string.Empty;
            MensagemErro = result.Message ?? string.Empty;
            await CarregarAsync();
        }
        finally
        {
            AGuardar = false;
        }
    }

    private async Task PagarAsync(object? parametro)
    {
        if (parametro is not ContaPagarListItemDto conta) return;

        MensagemErro = string.Empty;
        AProcessarPagamento = true;
        try
        {
            var result = await _service.PayAsync(conta.Id, OrigemPagamentoSelecionada, DataPagamento);
            if (result.IsFailure)
            {
                MensagemErro = BuildMessage(result.Message, result.Errors);
                return;
            }

            MensagemErro = result.Message ?? string.Empty;
            await CarregarAsync();
        }
        finally
        {
            AProcessarPagamento = false;
        }
    }

    private async Task CancelarAsync(object? parametro)
    {
        if (parametro is not ContaPagarListItemDto conta) return;

        var result = await _service.CancelAsync(conta.Id);
        MensagemErro = result.IsFailure
            ? BuildMessage(result.Message, result.Errors)
            : result.Message ?? string.Empty;

        if (result.IsSuccess) await CarregarAsync();
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source) target.Add(item);
    }

    private static string BuildMessage(string? message, IEnumerable<string> errors)
    {
        var details = string.Join(Environment.NewLine, errors);
        if (string.IsNullOrWhiteSpace(message)) return details;
        return string.IsNullOrWhiteSpace(details) ? message : $"{message}{Environment.NewLine}{details}";
    }
}
