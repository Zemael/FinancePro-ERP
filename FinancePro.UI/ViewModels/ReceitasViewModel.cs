using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class ReceitasViewModel : ViewModelBase
{
    private readonly IReceitasService _service;
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

    /// <summary>Origem partilhada usada sempre que se clica "Receber" numa linha.</summary>
    public OpcaoOrigemDto? OrigemRecebimentoSelecionada { get => _origemRecebimentoSelecionada; set => SetProperty(ref _origemRecebimentoSelecionada, value); }

    public ObservableCollection<ClienteOpcaoDto> Clientes { get; } = new();
    public ObservableCollection<CategoriaOpcaoDto> Categorias { get; } = new();
    public ObservableCollection<OpcaoOrigemDto> Origens { get; } = new();
    public ObservableCollection<ContaReceberListItemDto> Contas { get; } = new();

    public ICommand CriarCommand { get; }
    public ICommand ReceberCommand { get; }
    public ICommand CancelarCommand { get; }

    public ReceitasViewModel(IReceitasService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;

        CriarCommand = new AsyncRelayCommand(_ => CriarAsync(), _ => !AGuardar);
        ReceberCommand = new AsyncRelayCommand(ReceberAsync);
        CancelarCommand = new AsyncRelayCommand(CancelarAsync);

        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var clientes = await _service.ListarClientesAsync(_empresaId);
        Clientes.Clear();
        foreach (var cliente in clientes) Clientes.Add(cliente);

        var categorias = await _service.ListarCategoriasAsync(_empresaId);
        Categorias.Clear();
        foreach (var categoria in categorias) Categorias.Add(categoria);
        CategoriaSelecionada = Categorias.FirstOrDefault();

        var origens = await _service.ListarOrigensAsync(_empresaId);
        Origens.Clear();
        foreach (var origem in origens) Origens.Add(origem);
        OrigemRecebimentoSelecionada = Origens.FirstOrDefault(o => o.Disponivel);

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
        MensagemSucesso = string.Empty;

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
            await _service.CriarAsync(new NovaContaReceberDto
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

            Descricao = string.Empty;
            ValorTexto = string.Empty;

            await CarregarContasAsync();
            MensagemSucesso = "Conta a receber registada com sucesso.";
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

    private async Task ReceberAsync(object? parametro)
    {
        if (parametro is not ContaReceberListItemDto conta)
        {
            return;
        }

        MensagemErro = string.Empty;
        MensagemSucesso = string.Empty;

        if (OrigemRecebimentoSelecionada is null)
        {
            MensagemErro = "Selecione a origem do recebimento (caixa ou conta bancária) antes de continuar.";
            return;
        }

        if (!OrigemRecebimentoSelecionada.Disponivel)
        {
            MensagemErro = OrigemRecebimentoSelecionada.MotivoIndisponibilidade
                ?? "A origem selecionada não está disponível.";
            return;
        }

        try
        {
            await _service.RegistarRecebimentoAsync(
                conta.Id,
                OrigemRecebimentoSelecionada.Tipo,
                OrigemRecebimentoSelecionada.Id,
                DataRecebimento);

            await CarregarContasAsync();
            MensagemSucesso = $"Recebimento {conta.Codigo} confirmado e lançado na Tesouraria.";
        }
        catch (Exception ex)
        {
            MensagemErro = ex.Message;
        }
    }

    private async Task CancelarAsync(object? parametro)
    {
        MensagemErro = string.Empty;
        MensagemSucesso = string.Empty;

        if (parametro is not ContaReceberListItemDto conta)
        {
            return;
        }

        try
        {
            await _service.CancelarAsync(conta.Id);
            await CarregarContasAsync();
            MensagemSucesso = $"Conta {conta.Codigo} cancelada.";
        }
        catch (Exception ex)
        {
            MensagemErro = ex.Message;
        }
    }
}
