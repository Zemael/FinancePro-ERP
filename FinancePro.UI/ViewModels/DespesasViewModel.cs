using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
<<<<<<< HEAD
using FinancePro.Application.Payables;
=======
using FinancePro.Application.Expenses;
>>>>>>> origin/develop
using FinancePro.Core.DTOs;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class DespesasViewModel : ViewModelBase
{
<<<<<<< HEAD
    private readonly PayablesApplicationService _service;
=======
    private readonly ExpenseApplicationService _service;
>>>>>>> origin/develop
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
    private string _mensagemSucesso = string.Empty;
    private bool _aGuardar;
    private bool _aCarregar;
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
    public string MensagemSucesso { get => _mensagemSucesso; set => SetProperty(ref _mensagemSucesso, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }
    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }
    public DateTime DataPagamento { get => _dataPagamento; set => SetProperty(ref _dataPagamento, value); }
    public bool AProcessarPagamento { get => _aProcessarPagamento; set => SetProperty(ref _aProcessarPagamento, value); }
<<<<<<< HEAD
    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }
=======
>>>>>>> origin/develop
    public OpcaoOrigemDto? OrigemPagamentoSelecionada { get => _origemPagamentoSelecionada; set => SetProperty(ref _origemPagamentoSelecionada, value); }

    public ObservableCollection<FornecedorOpcaoDto> Fornecedores { get; } = new();
    public ObservableCollection<CategoriaOpcaoDto> Categorias { get; } = new();
    public ObservableCollection<OpcaoOrigemDto> Origens { get; } = new();
    public ObservableCollection<ContaPagarListItemDto> Contas { get; } = new();

    public ICommand CriarCommand { get; }
    public ICommand PagarCommand { get; }
    public ICommand CancelarCommand { get; }
    public ICommand AtualizarCommand { get; }

<<<<<<< HEAD
    public DespesasViewModel(PayablesApplicationService service, int empresaId)
=======
    public DespesasViewModel(ExpenseApplicationService service, int empresaId)
>>>>>>> origin/develop
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
<<<<<<< HEAD
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
=======
        LimparMensagens();
        ACarregar = true;
        try
        {
            var fornecedores = await _service.ListarFornecedoresAsync(_empresaId);
            if (fornecedores.IsFailure) { MensagemErro = string.Join(" ", fornecedores.Errors); return; }
            Fornecedores.Clear();
            foreach (var fornecedor in fornecedores.Value ?? Array.Empty<FornecedorOpcaoDto>()) Fornecedores.Add(fornecedor);

            var categorias = await _service.ListarCategoriasAsync(_empresaId);
            if (categorias.IsFailure) { MensagemErro = string.Join(" ", categorias.Errors); return; }
            Categorias.Clear();
            foreach (var categoria in categorias.Value ?? Array.Empty<CategoriaOpcaoDto>()) Categorias.Add(categoria);
            CategoriaSelecionada = Categorias.FirstOrDefault();

            var origens = await _service.ListarOrigensAsync(_empresaId);
            if (origens.IsFailure) { MensagemErro = string.Join(" ", origens.Errors); return; }
            Origens.Clear();
            foreach (var origem in origens.Value ?? Array.Empty<OpcaoOrigemDto>()) Origens.Add(origem);
            OrigemPagamentoSelecionada = Origens.FirstOrDefault(o => o.Disponivel);

            await CarregarContasAsync();
>>>>>>> origin/develop
        }
        finally
        {
            ACarregar = false;
        }
<<<<<<< HEAD
=======
    }

    private async Task CarregarContasAsync()
    {
        var resultado = await _service.ListarAsync(_empresaId);
        if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }
        Contas.Clear();
        foreach (var conta in resultado.Value ?? Array.Empty<ContaPagarListItemDto>()) Contas.Add(conta);
>>>>>>> origin/develop
    }

    private async Task CriarAsync()
    {
<<<<<<< HEAD
        MensagemErro = string.Empty;
        if (!decimal.TryParse(ValorTexto, NumberStyles.Number, CultureInfo.CurrentCulture, out var valor) || valor <= 0)
=======
        LimparMensagens();

        if (!decimal.TryParse(ValorTexto, out var valor))
>>>>>>> origin/develop
        {
            MensagemErro = "Indique um valor válido.";
            return;
        }

        AGuardar = true;
        try
        {
<<<<<<< HEAD
            var result = await _service.CreateAsync(new NovaContaPagarDto
=======
            var resultado = await _service.CriarAsync(new NovaContaPagarDto
>>>>>>> origin/develop
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

<<<<<<< HEAD
            if (result.IsFailure)
            {
                MensagemErro = BuildMessage(result.Message, result.Errors);
                return;
            }

            Descricao = string.Empty;
            ValorTexto = string.Empty;
            MensagemErro = result.Message ?? string.Empty;
            await CarregarAsync();
=======
            if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }

            Descricao = string.Empty;
            ValorTexto = string.Empty;
            await CarregarContasAsync();
            MensagemSucesso = resultado.Message ?? "Conta a pagar registada com sucesso.";
>>>>>>> origin/develop
        }
        finally
        {
            AGuardar = false;
        }
    }

    private async Task PagarAsync(object? parametro)
    {
<<<<<<< HEAD
        if (parametro is not ContaPagarListItemDto conta) return;
=======
        LimparMensagens();
        if (parametro is not ContaPagarListItemDto conta) return;
        if (OrigemPagamentoSelecionada is null)
        {
            MensagemErro = "Selecione a origem do pagamento antes de continuar.";
            return;
        }
>>>>>>> origin/develop

        AProcessarPagamento = true;
        try
        {
<<<<<<< HEAD
            var result = await _service.PayAsync(conta.Id, OrigemPagamentoSelecionada, DataPagamento);
            if (result.IsFailure)
            {
                MensagemErro = BuildMessage(result.Message, result.Errors);
                return;
            }

            MensagemErro = result.Message ?? string.Empty;
            await CarregarAsync();
=======
            var resultado = await _service.RegistarPagamentoAsync(
                conta.Id,
                OrigemPagamentoSelecionada.Tipo,
                OrigemPagamentoSelecionada.Id,
                DataPagamento);

            if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }
            await CarregarContasAsync();
            MensagemSucesso = resultado.Message ?? $"Pagamento {conta.Codigo} confirmado.";
>>>>>>> origin/develop
        }
        finally
        {
            AProcessarPagamento = false;
        }
    }

    private async Task CancelarAsync(object? parametro)
    {
<<<<<<< HEAD
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
=======
        LimparMensagens();
        if (parametro is not ContaPagarListItemDto conta) return;

        var resultado = await _service.CancelarAsync(conta.Id);
        if (resultado.IsFailure) { MensagemErro = string.Join(" ", resultado.Errors); return; }
        await CarregarContasAsync();
        MensagemSucesso = resultado.Message ?? $"Conta {conta.Codigo} cancelada.";
    }

    private void LimparMensagens()
    {
        MensagemErro = string.Empty;
        MensagemSucesso = string.Empty;
>>>>>>> origin/develop
    }
}
