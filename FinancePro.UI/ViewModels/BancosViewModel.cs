using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class BancosViewModel : ViewModelBase
{
    private readonly IBancoService _service;
    private readonly int _empresaId;

    private string _nomeBanco = string.Empty;
    private string _swiftBanco = string.Empty;
    private string _mensagemErroBanco = string.Empty;
    private bool _aGuardarBanco;

    private BancoListItemDto? _bancoSelecionado;
    private string _numeroConta = string.Empty;
    private string _iban = string.Empty;
    private string _titular = string.Empty;
    private string _saldoInicialTexto = string.Empty;
    private string _mensagemErroConta = string.Empty;
    private bool _aGuardarConta;
    private DateTime _ultimaAtualizacao = DateTime.Now;

    public string NomeBanco { get => _nomeBanco; set => SetProperty(ref _nomeBanco, value); }
    public string SwiftBanco { get => _swiftBanco; set => SetProperty(ref _swiftBanco, value); }
    public string MensagemErroBanco { get => _mensagemErroBanco; set => SetProperty(ref _mensagemErroBanco, value); }
    public bool AGuardarBanco { get => _aGuardarBanco; set => SetProperty(ref _aGuardarBanco, value); }
    public ObservableCollection<BancoListItemDto> Bancos { get; } = new();
    public ICommand CriarBancoCommand { get; }

    public BancoListItemDto? BancoSelecionado { get => _bancoSelecionado; set => SetProperty(ref _bancoSelecionado, value); }
    public string NumeroConta { get => _numeroConta; set => SetProperty(ref _numeroConta, value); }
    public string IBAN { get => _iban; set => SetProperty(ref _iban, value); }
    public string Titular { get => _titular; set => SetProperty(ref _titular, value); }
    public string SaldoInicialTexto { get => _saldoInicialTexto; set => SetProperty(ref _saldoInicialTexto, value); }
    public string MensagemErroConta { get => _mensagemErroConta; set => SetProperty(ref _mensagemErroConta, value); }
    public bool AGuardarConta { get => _aGuardarConta; set => SetProperty(ref _aGuardarConta, value); }
    public ObservableCollection<ContaBancariaListItemDto> Contas { get; } = new();
    public ICommand CriarContaCommand { get; }
    public ICommand AlternarAtivoContaCommand { get; }
    public ICommand AtualizarCommand { get; }

    public int TotalBancos => Bancos.Count;
    public int TotalContas => Contas.Count;
    public int ContasAtivas => Contas.Count(c => c.Ativo);
    public decimal SaldoInicialTotal => Contas.Sum(c => c.SaldoInicial);
    public DateTime UltimaAtualizacao { get => _ultimaAtualizacao; private set => SetProperty(ref _ultimaAtualizacao, value); }

    public BancosViewModel(IBancoService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;

        CriarBancoCommand = new AsyncRelayCommand(_ => CriarBancoAsync(), _ => !AGuardarBanco);
        CriarContaCommand = new AsyncRelayCommand(_ => CriarContaAsync(), _ => !AGuardarConta);
        AlternarAtivoContaCommand = new AsyncRelayCommand(AlternarAtivoContaAsync);
        AtualizarCommand = new AsyncRelayCommand(_ => CarregarAsync());

        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        await CarregarBancosAsync();
        await CarregarContasAsync();
        UltimaAtualizacao = DateTime.Now;
        OnPropertyChanged(nameof(TotalBancos));
        OnPropertyChanged(nameof(TotalContas));
        OnPropertyChanged(nameof(ContasAtivas));
        OnPropertyChanged(nameof(SaldoInicialTotal));
    }

    private async Task CarregarBancosAsync()
    {
        var bancos = await _service.ListarBancosAsync();
        Bancos.Clear();
        foreach (var banco in bancos)
        {
            Bancos.Add(banco);
        }
        BancoSelecionado = Bancos.FirstOrDefault();
    }

    private async Task CarregarContasAsync()
    {
        var contas = await _service.ListarContasAsync(_empresaId);
        Contas.Clear();
        foreach (var conta in contas)
        {
            Contas.Add(conta);
        }
    }

    private async Task CriarBancoAsync()
    {
        MensagemErroBanco = string.Empty;

        if (string.IsNullOrWhiteSpace(NomeBanco))
        {
            MensagemErroBanco = "Indique o nome do banco.";
            return;
        }

        AGuardarBanco = true;
        try
        {
            await _service.CriarBancoAsync(new NovoBancoDto { Nome = NomeBanco, CodigoSwift = SwiftBanco });
            NomeBanco = string.Empty;
            SwiftBanco = string.Empty;
            await CarregarBancosAsync();
        }
        catch (Exception ex)
        {
            MensagemErroBanco = ex.Message;
        }
        finally
        {
            AGuardarBanco = false;
        }
    }

    private async Task CriarContaAsync()
    {
        MensagemErroConta = string.Empty;

        if (BancoSelecionado is null)
        {
            MensagemErroConta = "Selecione o banco.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NumeroConta))
        {
            MensagemErroConta = "Indique o número de conta.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Titular))
        {
            MensagemErroConta = "Indique o titular da conta.";
            return;
        }

        decimal.TryParse(SaldoInicialTexto, out var saldoInicial);

        AGuardarConta = true;
        try
        {
            await _service.CriarContaAsync(new NovaContaBancariaDto
            {
                BancoId = BancoSelecionado.Id,
                NumeroConta = NumeroConta,
                IBAN = IBAN,
                Titular = Titular,
                SaldoInicial = saldoInicial,
                EmpresaId = _empresaId
            });

            NumeroConta = string.Empty;
            IBAN = string.Empty;
            Titular = string.Empty;
            SaldoInicialTexto = string.Empty;

            await CarregarContasAsync();
        }
        catch (Exception ex)
        {
            MensagemErroConta = ex.Message;
        }
        finally
        {
            AGuardarConta = false;
        }
    }

    private async Task AlternarAtivoContaAsync(object? parametro)
    {
        if (parametro is not ContaBancariaListItemDto conta)
        {
            return;
        }

        await _service.AlternarAtivoContaAsync(conta.Id, !conta.Ativo);
        await CarregarContasAsync();
    }
}
