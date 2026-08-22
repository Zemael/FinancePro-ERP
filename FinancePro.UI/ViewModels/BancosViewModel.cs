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
    private string _siglaBanco = string.Empty;
    private string _swiftBanco = string.Empty;
    private string _enderecoBanco = string.Empty;
    private string _contactoBanco = string.Empty;
    private int _bancoIdEdicao;
    private string _mensagemErroBanco = string.Empty;
    private bool _aGuardarBanco;
    private BancoListItemDto? _bancoSelecionado;
    private string _numeroConta = string.Empty;
    private string _iban = string.Empty;
    private string _titular = string.Empty;
    private string _saldoInicialTexto = string.Empty;
    private string _mensagemErroConta = string.Empty;
    private bool _aGuardarConta;
    private int _contaIdEdicao;
    private string _moedaContaEdicao = "FCFA";

    public string NomeBanco { get => _nomeBanco; set => SetProperty(ref _nomeBanco, value); }
    public string SiglaBanco { get => _siglaBanco; set => SetProperty(ref _siglaBanco, value); }
    public string SwiftBanco { get => _swiftBanco; set => SetProperty(ref _swiftBanco, value); }
    public string EnderecoBanco { get => _enderecoBanco; set => SetProperty(ref _enderecoBanco, value); }
    public string ContactoBanco { get => _contactoBanco; set => SetProperty(ref _contactoBanco, value); }
    public bool EmEdicaoBanco => _bancoIdEdicao > 0;
    public string TextoAcaoBanco => EmEdicaoBanco ? "Guardar alterações" : "Adicionar banco";
    public string MensagemErroBanco { get => _mensagemErroBanco; set => SetProperty(ref _mensagemErroBanco, value); }
    public bool AGuardarBanco { get => _aGuardarBanco; set => SetProperty(ref _aGuardarBanco, value); }
    public ObservableCollection<BancoListItemDto> Bancos { get; } = new();
    public ICommand CriarBancoCommand { get; }
    public ICommand EditarBancoCommand { get; }
    public ICommand NovoBancoCommand { get; }
    public BancoListItemDto? BancoSelecionado { get => _bancoSelecionado; set => SetProperty(ref _bancoSelecionado, value); }
    public string NumeroConta { get => _numeroConta; set => SetProperty(ref _numeroConta, value); }
    public string IBAN { get => _iban; set => SetProperty(ref _iban, value); }
    public string Titular { get => _titular; set => SetProperty(ref _titular, value); }
    public string SaldoInicialTexto { get => _saldoInicialTexto; set => SetProperty(ref _saldoInicialTexto, value); }
    public string MensagemErroConta { get => _mensagemErroConta; set => SetProperty(ref _mensagemErroConta, value); }
    public bool AGuardarConta { get => _aGuardarConta; set => SetProperty(ref _aGuardarConta, value); }
    public ObservableCollection<ContaBancariaListItemDto> Contas { get; } = new();
    public ICommand CriarContaCommand { get; }
    public ICommand EditarContaCommand { get; }
    public ICommand AlternarAtivoContaCommand { get; }
    public ICommand AtualizarCommand { get; }

    public BancosViewModel(BankingMasterDataService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;
        CriarBancoCommand = new AsyncRelayCommand(_ => CriarBancoAsync(), _ => !AGuardarBanco);
        EditarBancoCommand = new RelayCommand(EditarBanco);
        NovoBancoCommand = new RelayCommand(_ => LimparBanco());
        CriarContaCommand = new AsyncRelayCommand(_ => CriarContaAsync(), _ => !AGuardarConta);
        EditarContaCommand = new RelayCommand(EditarConta);
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
            var result = EmEdicaoBanco
                ? await _service.UpdateBankAsync(_bancoIdEdicao, NomeBanco, SiglaBanco, SwiftBanco, EnderecoBanco, ContactoBanco)
                : await _service.CreateBankAsync(NomeBanco, SiglaBanco, SwiftBanco, EnderecoBanco, ContactoBanco);
            if (result.IsFailure) { MensagemErroBanco = string.Join(Environment.NewLine, result.Errors); return; }
            LimparBanco();
            MensagemErroBanco = result.Message ?? string.Empty;
            await CarregarBancosAsync();
        }
        finally { AGuardarBanco = false; }
    }

    private void EditarBanco(object? parametro)
    {
        if (parametro is not BancoListItemDto banco) return;
        _bancoIdEdicao = banco.Id;
        NomeBanco = banco.Nome;
        SiglaBanco = banco.Sigla ?? string.Empty;
        SwiftBanco = banco.CodigoSwift ?? string.Empty;
        EnderecoBanco = banco.Endereco ?? string.Empty;
        ContactoBanco = banco.Contacto ?? string.Empty;
        MensagemErroBanco = "A editar o banco selecionado.";
        OnPropertyChanged(nameof(EmEdicaoBanco));
        OnPropertyChanged(nameof(TextoAcaoBanco));
    }

    private void LimparBanco()
    {
        _bancoIdEdicao = 0;
        NomeBanco = SiglaBanco = SwiftBanco = EnderecoBanco = ContactoBanco = string.Empty;
        MensagemErroBanco = string.Empty;
        OnPropertyChanged(nameof(EmEdicaoBanco));
        OnPropertyChanged(nameof(TextoAcaoBanco));
    }

    private async Task CriarContaAsync()
    {
        MensagemErroConta = string.Empty;
        decimal.TryParse(SaldoInicialTexto, out var saldoInicial);
        AGuardarConta = true;
        try
        {
            var request = new NovaContaBancariaDto
            {
                BancoId = BancoSelecionado?.Id ?? 0,
                NumeroConta = NumeroConta,
                IBAN = IBAN,
                Titular = Titular,
                SaldoInicial = saldoInicial,
                Moeda = _moedaContaEdicao,
                EmpresaId = _empresaId
            };
            var result = _contaIdEdicao > 0
                ? await _service.UpdateAccountAsync(_contaIdEdicao, request)
                : await _service.CreateAccountAsync(request);
            if (result.IsFailure) { MensagemErroConta = string.Join(Environment.NewLine, result.Errors); return; }
            _contaIdEdicao = 0;
            _moedaContaEdicao = "FCFA";
            NumeroConta = IBAN = Titular = SaldoInicialTexto = string.Empty;
            MensagemErroConta = result.Message ?? string.Empty;
            await CarregarContasAsync();
        }
        finally { AGuardarConta = false; }
    }

    private void EditarConta(object? parametro)
    {
        if (parametro is not ContaBancariaListItemDto conta) return;
        _contaIdEdicao = conta.Id;
        BancoSelecionado = Bancos.FirstOrDefault(b => b.Nome == conta.BancoNome) ?? BancoSelecionado;
        NumeroConta = conta.NumeroConta;
        IBAN = conta.IBAN ?? string.Empty;
        Titular = conta.Titular;
        SaldoInicialTexto = conta.SaldoInicial.ToString("0.##");
        _moedaContaEdicao = conta.Moeda;
        MensagemErroConta = "Conta bancária carregada para edição.";
    }

    private async Task AlternarAtivoContaAsync(object? parametro)
    {
        if (parametro is not ContaBancariaListItemDto conta) return;
        var result = await _service.SetAccountActiveAsync(conta.Id, !conta.Ativo);
        MensagemErroConta = result.IsSuccess ? result.Message ?? string.Empty : string.Join(Environment.NewLine, result.Errors);
        if (result.IsSuccess) await CarregarContasAsync();
    }
}
