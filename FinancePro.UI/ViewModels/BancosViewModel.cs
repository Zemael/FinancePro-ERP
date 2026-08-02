using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Application.MasterData.Banking;
using FinancePro.Core.DTOs;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class BancosViewModel : ViewModelBase
{
    private readonly BankingMasterDataService _service;
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

    public BancosViewModel(BankingMasterDataService service, int empresaId)
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
    }

    private async Task CarregarBancosAsync()
    {
        MensagemErroBanco = string.Empty;
        var result = await _service.ListBanksAsync();
        if (result.IsFailure) { MensagemErroBanco = string.Join(Environment.NewLine, result.Errors); return; }
        Bancos.Clear();
        foreach (var banco in result.Value ?? Array.Empty<BancoListItemDto>()) Bancos.Add(banco);
        BancoSelecionado = Bancos.FirstOrDefault();
    }

    private async Task CarregarContasAsync()
    {
        MensagemErroConta = string.Empty;
        var result = await _service.ListAccountsAsync(_empresaId);
        if (result.IsFailure) { MensagemErroConta = string.Join(Environment.NewLine, result.Errors); return; }
        Contas.Clear();
        foreach (var conta in result.Value ?? Array.Empty<ContaBancariaListItemDto>()) Contas.Add(conta);
    }

    private async Task CriarBancoAsync()
    {
        MensagemErroBanco = string.Empty;
        AGuardarBanco = true;
        try
        {
            var result = await _service.CreateBankAsync(NomeBanco, SwiftBanco);
            if (result.IsFailure) { MensagemErroBanco = string.Join(Environment.NewLine, result.Errors); return; }
            NomeBanco = string.Empty;
            SwiftBanco = string.Empty;
            MensagemErroBanco = result.Message ?? string.Empty;
            await CarregarBancosAsync();
        }
        finally { AGuardarBanco = false; }
    }

    private async Task CriarContaAsync()
    {
        MensagemErroConta = string.Empty;
        decimal.TryParse(SaldoInicialTexto, out var saldoInicial);
        AGuardarConta = true;
        try
        {
            var result = await _service.CreateAccountAsync(new NovaContaBancariaDto
            {
                BancoId = BancoSelecionado?.Id ?? 0,
                NumeroConta = NumeroConta,
                IBAN = IBAN,
                Titular = Titular,
                SaldoInicial = saldoInicial,
                EmpresaId = _empresaId
            });
            if (result.IsFailure) { MensagemErroConta = string.Join(Environment.NewLine, result.Errors); return; }
            NumeroConta = IBAN = Titular = SaldoInicialTexto = string.Empty;
            MensagemErroConta = result.Message ?? string.Empty;
            await CarregarContasAsync();
        }
        finally { AGuardarConta = false; }
    }

    private async Task AlternarAtivoContaAsync(object? parametro)
    {
        if (parametro is not ContaBancariaListItemDto conta) return;
        var result = await _service.SetAccountActiveAsync(conta.Id, !conta.Ativo);
        MensagemErroConta = result.IsSuccess ? result.Message ?? string.Empty : string.Join(Environment.NewLine, result.Errors);
        if (result.IsSuccess) await CarregarContasAsync();
    }
}
