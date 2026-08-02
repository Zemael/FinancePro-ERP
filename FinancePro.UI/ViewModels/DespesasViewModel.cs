using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class DespesasViewModel : ViewModelBase
{
    private readonly IDespesasService _service;
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

    /// <summary>Origem partilhada usada sempre que se clica "Pagar" numa linha.</summary>
    public OpcaoOrigemDto? OrigemPagamentoSelecionada { get => _origemPagamentoSelecionada; set => SetProperty(ref _origemPagamentoSelecionada, value); }

    public ObservableCollection<FornecedorOpcaoDto> Fornecedores { get; } = new();
    public ObservableCollection<CategoriaOpcaoDto> Categorias { get; } = new();
    public ObservableCollection<OpcaoOrigemDto> Origens { get; } = new();
    public ObservableCollection<ContaPagarListItemDto> Contas { get; } = new();

    public ICommand CriarCommand { get; }
    public ICommand PagarCommand { get; }
    public ICommand CancelarCommand { get; }

    public DespesasViewModel(IDespesasService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;

        CriarCommand = new AsyncRelayCommand(_ => CriarAsync(), _ => !AGuardar);
        PagarCommand = new AsyncRelayCommand(PagarAsync);
        CancelarCommand = new AsyncRelayCommand(CancelarAsync);

        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var fornecedores = await _service.ListarFornecedoresAsync(_empresaId);
        Fornecedores.Clear();
        foreach (var f in fornecedores) Fornecedores.Add(f);

        var categorias = await _service.ListarCategoriasAsync(_empresaId);
        Categorias.Clear();
        foreach (var c in categorias) Categorias.Add(c);
        CategoriaSelecionada = Categorias.FirstOrDefault();

        var origens = await _service.ListarOrigensAsync(_empresaId);
        Origens.Clear();
        foreach (var o in origens) Origens.Add(o);
        OrigemPagamentoSelecionada = Origens.FirstOrDefault();

        await CarregarContasAsync();
    }

    private async Task CarregarContasAsync()
    {
        var contas = await _service.ListarAsync(_empresaId);
        Contas.Clear();
        foreach (var conta in contas) Contas.Add(conta);
    }

    private async Task CriarAsync()
    {
        MensagemErro = string.Empty;

        if (string.IsNullOrWhiteSpace(Descricao))
        {
            MensagemErro = "Indique uma descrição.";
            return;
        }

        if (!decimal.TryParse(ValorTexto, out var valor) || valor <= 0)
        {
            MensagemErro = "Indique um valor válido.";
            return;
        }

        AGuardar = true;
        try
        {
            await _service.CriarAsync(new NovaContaPagarDto
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

            Descricao = string.Empty;
            ValorTexto = string.Empty;

            await CarregarContasAsync();
        }
        catch (Exception ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            AGuardar = false;
        }
    }

    private async Task PagarAsync(object? parametro)
    {
        if (parametro is not ContaPagarListItemDto conta)
        {
            return;
        }

        if (OrigemPagamentoSelecionada is null)
        {
            MensagemErro = "Selecione a origem do pagamento (caixa ou conta bancária) antes de continuar.";
            return;
        }

        MensagemErro = string.Empty;
        AProcessarPagamento = true;
        try
        {
            await _service.RegistarPagamentoAsync(
                conta.Id,
                OrigemPagamentoSelecionada.Tipo,
                OrigemPagamentoSelecionada.Id,
                DataPagamento);

            await CarregarContasAsync();
        }
        catch (Exception ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            AProcessarPagamento = false;
        }
    }

    private async Task CancelarAsync(object? parametro)
    {
        if (parametro is not ContaPagarListItemDto conta)
        {
            return;
        }

        await _service.CancelarAsync(conta.Id);
        await CarregarContasAsync();
    }
}
