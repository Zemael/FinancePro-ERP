using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Application.Purchasing;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class CompraViewModel : ViewModelBase
{
    private readonly PurchasingApplicationService _service;
    private readonly int _empresaId;

    private DateTime _data = DateTime.Today;
    private string _departamento = string.Empty;
    private string _centroCusto = string.Empty;
    private string _projeto = string.Empty;
    private string _comprador = string.Empty;
    private PrioridadeCompra _prioridade = PrioridadeCompra.Normal;
    private string _valorTotalTexto = string.Empty;
    private FornecedorOpcaoDto? _fornecedorSelecionado;
    private string _mensagemErro = string.Empty;
    private string _mensagemSucesso = string.Empty;
    private bool _aGuardar;
    private bool _aCarregar;
    private bool _aProcessarAcao;

    public DateTime Data { get => _data; set => SetProperty(ref _data, value); }
    public string Departamento { get => _departamento; set => SetProperty(ref _departamento, value); }
    public string CentroCusto { get => _centroCusto; set => SetProperty(ref _centroCusto, value); }
    public string Projeto { get => _projeto; set => SetProperty(ref _projeto, value); }
    public string Comprador { get => _comprador; set => SetProperty(ref _comprador, value); }
    public PrioridadeCompra Prioridade { get => _prioridade; set => SetProperty(ref _prioridade, value); }
    public string ValorTotalTexto { get => _valorTotalTexto; set => SetProperty(ref _valorTotalTexto, value); }
    public FornecedorOpcaoDto? FornecedorSelecionado { get => _fornecedorSelecionado; set => SetProperty(ref _fornecedorSelecionado, value); }
    public string MensagemErro { get => _mensagemErro; set => SetProperty(ref _mensagemErro, value); }
    public string MensagemSucesso { get => _mensagemSucesso; set => SetProperty(ref _mensagemSucesso, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }
    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }
    public bool AProcessarAcao { get => _aProcessarAcao; set => SetProperty(ref _aProcessarAcao, value); }

    public IReadOnlyList<PrioridadeCompra> PrioridadesDisponiveis { get; } = Enum.GetValues<PrioridadeCompra>().ToList();

    public ObservableCollection<FornecedorOpcaoDto> Fornecedores { get; } = new();
    public ObservableCollection<CompraListItemDto> Compras { get; } = new();

    public ICommand CriarCommand { get; }
    public ICommand AprovarCommand { get; }
    public ICommand RejeitarCommand { get; }
    public ICommand CancelarCommand { get; }
    public ICommand AtualizarCommand { get; }

    public CompraViewModel(PurchasingApplicationService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;

        CriarCommand = new AsyncRelayCommand(_ => CriarAsync(), _ => !AGuardar);
        AprovarCommand = new AsyncRelayCommand(p => ExecutarAcaoAsync(p, _service.AprovarAsync), _ => !AProcessarAcao);
        RejeitarCommand = new AsyncRelayCommand(p => ExecutarAcaoAsync(p, _service.RejeitarAsync), _ => !AProcessarAcao);
        CancelarCommand = new AsyncRelayCommand(p => ExecutarAcaoAsync(p, _service.CancelarAsync), _ => !AProcessarAcao);
        AtualizarCommand = new AsyncRelayCommand(_ => CarregarAsync(), _ => !ACarregar);

        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        LimparMensagens();
        ACarregar = true;
        try
        {
            var fornecedores = await _service.ListarFornecedoresAsync(_empresaId);
            if (fornecedores.IsFailure)
            {
                MensagemErro = string.Join(" ", fornecedores.Errors);
                return;
            }

            Fornecedores.Clear();
            foreach (var fornecedor in fornecedores.Value ?? Array.Empty<FornecedorOpcaoDto>())
                Fornecedores.Add(fornecedor);

            await CarregarComprasAsync();
        }
        finally
        {
            ACarregar = false;
        }
    }

    private async Task CarregarComprasAsync()
    {
        var compras = await _service.ListarAsync(_empresaId);
        if (compras.IsFailure)
        {
            MensagemErro = string.Join(" ", compras.Errors);
            return;
        }

        Compras.Clear();
        foreach (var compra in compras.Value ?? Array.Empty<CompraListItemDto>())
            Compras.Add(compra);
    }

    private async Task CriarAsync()
    {
        LimparMensagens();

        if (!decimal.TryParse(ValorTotalTexto, out var valor))
        {
            MensagemErro = "Indique um valor total válido.";
            return;
        }

        AGuardar = true;
        try
        {
            var resultado = await _service.CriarAsync(new NovaCompraDto
            {
                Data = Data,
                Departamento = Departamento,
                CentroCusto = CentroCusto,
                Projeto = Projeto,
                Comprador = Comprador,
                Prioridade = Prioridade,
                ValorTotal = valor,
                FornecedorId = FornecedorSelecionado?.Id,
                EmpresaId = _empresaId
            });

            if (resultado.IsFailure)
            {
                MensagemErro = string.Join(" ", resultado.Errors);
                return;
            }

            MensagemSucesso = resultado.Message ?? "Pedido de compra criado com sucesso.";
            LimparFormulario();
            await CarregarComprasAsync();
        }
        finally
        {
            AGuardar = false;
        }
    }

    private async Task ExecutarAcaoAsync(
        object? parametro,
        Func<int, Task<FinancePro.Application.Common.Results.Result>> acao)
    {
        if (parametro is not CompraListItemDto compra)
            return;

        LimparMensagens();
        AProcessarAcao = true;
        try
        {
            var resultado = await acao(compra.Id);
            if (resultado.IsFailure)
            {
                MensagemErro = string.Join(" ", resultado.Errors);
                return;
            }

            MensagemSucesso = resultado.Message ?? "Operação concluída.";
            await CarregarComprasAsync();
        }
        finally
        {
            AProcessarAcao = false;
        }
    }

    private void LimparFormulario()
    {
        Data = DateTime.Today;
        Departamento = string.Empty;
        CentroCusto = string.Empty;
        Projeto = string.Empty;
        Comprador = string.Empty;
        Prioridade = PrioridadeCompra.Normal;
        ValorTotalTexto = string.Empty;
        FornecedorSelecionado = null;
    }

    private void LimparMensagens()
    {
        MensagemErro = string.Empty;
        MensagemSucesso = string.Empty;
    }
}
