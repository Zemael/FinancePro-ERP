using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class EmpresasViewModel : ViewModelBase
{
    private readonly IEmpresaService _service;
    private string _pesquisa = string.Empty;
    private EmpresaListItemDto? _selecionada;
    private int _idEdicao;
    private string _nome = string.Empty;
    private string _nif = string.Empty;
    private string _telefone = string.Empty;
    private string _email = string.Empty;
    private string _morada = string.Empty;
    private string _moeda = "FCFA";
    private string _mensagem = string.Empty;
    private bool _aGuardar;

    public ObservableCollection<EmpresaListItemDto> Empresas { get; } = new();
    public string Pesquisa { get => _pesquisa; set => SetProperty(ref _pesquisa, value); }
    public EmpresaListItemDto? Selecionada { get => _selecionada; set => SetProperty(ref _selecionada, value); }
    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string NIF { get => _nif; set => SetProperty(ref _nif, value); }
    public string Telefone { get => _telefone; set => SetProperty(ref _telefone, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Morada { get => _morada; set => SetProperty(ref _morada, value); }
    public string Moeda { get => _moeda; set => SetProperty(ref _moeda, value); }
    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }

    public ICommand PesquisarCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand AlternarAtivoCommand { get; }

    public EmpresasViewModel(IEmpresaService service)
    {
        _service = service;
        PesquisarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        NovoCommand = new RelayCommand(_ => LimparFormulario());
        EditarCommand = new AsyncRelayCommand(_ => EditarAsync(), _ => Selecionada is not null);
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => !AGuardar);
        AlternarAtivoCommand = new AsyncRelayCommand(AlternarAtivoAsync);
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var lista = await _service.ListarAsync(Pesquisa);
        Empresas.Clear();
        foreach (var item in lista) Empresas.Add(item);
    }

    private async Task EditarAsync()
    {
        if (Selecionada is null) return;
        var dto = await _service.ObterAsync(Selecionada.Id);
        if (dto is null) return;
        _idEdicao = dto.Id;
        Nome = dto.Nome;
        NIF = dto.NIF ?? string.Empty;
        Telefone = dto.Telefone ?? string.Empty;
        Email = dto.Email ?? string.Empty;
        Morada = dto.Morada ?? string.Empty;
        Moeda = dto.Moeda;
        Mensagem = "Empresa carregada para edição.";
    }

    private async Task GuardarAsync()
    {
        Mensagem = string.Empty;
        AGuardar = true;
        try
        {
            await _service.GuardarAsync(new EmpresaDto
            {
                Id = _idEdicao,
                Nome = Nome,
                NIF = NIF,
                Telefone = Telefone,
                Email = Email,
                Morada = Morada,
                Moeda = Moeda
            });
            LimparFormulario();
            Mensagem = "Empresa guardada com sucesso.";
            await CarregarAsync();
        }
        catch (Exception ex) { Mensagem = ex.Message; }
        finally { AGuardar = false; }
    }

    private async Task AlternarAtivoAsync(object? parametro)
    {
        if (parametro is not EmpresaListItemDto empresa) return;
        await _service.AlternarAtivoAsync(empresa.Id, !empresa.Ativo);
        await CarregarAsync();
    }

    private void LimparFormulario()
    {
        _idEdicao = 0;
        Nome = NIF = Telefone = Email = Morada = string.Empty;
        Moeda = "FCFA";
        Mensagem = string.Empty;
    }
}
