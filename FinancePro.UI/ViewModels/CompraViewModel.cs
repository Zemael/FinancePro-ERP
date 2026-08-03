using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class CompraViewModel : ViewModelBase
{
    private readonly ICompraService _service;
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
    private bool _aGuardar;

    public DateTime Data { get => _data; set => SetProperty(ref _data, value); }
    public string Departamento { get => _departamento; set => SetProperty(ref _departamento, value); }
    public string CentroCusto { get => _centroCusto; set => SetProperty(ref _centroCusto, value); }
    public string Projeto { get => _projeto; set => SetProperty(ref _projeto, value); }
    public string Comprador { get => _comprador; set => SetProperty(ref _comprador, value); }
    public PrioridadeCompra Prioridade { get => _prioridade; set => SetProperty(ref _prioridade, value); }
    public string ValorTotalTexto { get => _valorTotalTexto; set => SetProperty(ref _valorTotalTexto, value); }
    public FornecedorOpcaoDto? FornecedorSelecionado { get => _fornecedorSelecionado; set => SetProperty(ref _fornecedorSelecionado, value); }
    public string MensagemErro { get => _mensagemErro; set => SetProperty(ref _mensagemErro, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }

    public IReadOnlyList<PrioridadeCompra> PrioridadesDisponiveis { get; } = Enum.GetValues<PrioridadeCompra>().ToList();

    public ObservableCollection<FornecedorOpcaoDto> Fornecedores { get; } = new();
    public ObservableCollection<CompraListItemDto> Compras { get; } = new();

    public ICommand CriarCommand { get; }
    public ICommand AprovarCommand { get; }
    public ICommand RejeitarCommand { get; }
    public ICommand CancelarCommand { get; }

    public CompraViewModel(ICompraService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;

        CriarCommand = new AsyncRelayCommand(_ => CriarAsync(), _ => !AGuardar);
        AprovarCommand = new AsyncRelayCommand(p => ExecutarAcaoAsync(p, _service.AprovarAsync));
        RejeitarCommand = new AsyncRelayCommand(p => ExecutarAcaoAsync(p, _service.RejeitarAsync));
        CancelarCommand = new AsyncRelayCommand(p => ExecutarAcaoAsync(p, _service.CancelarAsync));

        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var fornecedores = await _service.ListarFornecedoresAsync(_empresaId);
        Fornecedores.Clear();
        foreach (var f in fornecedores) Fornecedores.Add(f);

        await CarregarComprasAsync();
    }

    private async Task CarregarComprasAsync()
    {
        var compras = await _service.ListarAsync(_empresaId);
        Compras.Clear();
        foreach (var c in compras) Compras.Add(c);
    }

    private async Task CriarAsync()
    {
        MensagemErro = string.Empty;

        if (!decimal.TryParse(ValorTotalTexto, out var valor) || valor < 0)
        {
            MensagemErro = "Indique um valor total válido.";
            return;
        }

        AGuardar = true;
        try
        {
            await _service.CriarAsync(new NovaCompraDto
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

            Departamento = string.Empty;
            CentroCusto = string.Empty;
            Projeto = string.Empty;
            Comprador = string.Empty;
            ValorTotalTexto = string.Empty;

            await CarregarComprasAsync();
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

    private async Task ExecutarAcaoAsync(object? parametro, Func<int, Task> acao)
    {
        if (parametro is not CompraListItemDto compra)
        {
            return;
        }

        try
        {
            await acao(compra.Id);
            await CarregarComprasAsync();
        }
        catch (Exception ex)
        {
            MensagemErro = ex.Message;
        }
    }
}
