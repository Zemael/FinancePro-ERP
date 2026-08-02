using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class UtilizadoresViewModel : ViewModelBase
{
    private readonly IUtilizadorService _service;
    private readonly IPerfilService _perfilService;
    private readonly IEmpresaService _empresaService;
    private string _pesquisa = string.Empty;
    private UtilizadorDto? _selecionado;
    private int _idEdicao;
    private string _nomeCompleto = string.Empty;
    private string _email = string.Empty;
    private string _novaPassword = string.Empty;
    private int _perfilId;
    private int _empresaId;
    private string _mensagem = string.Empty;
    private bool _aGuardar;

    public ObservableCollection<UtilizadorDto> Utilizadores { get; } = new();
    public ObservableCollection<PerfilDto> Perfis { get; } = new();
    public ObservableCollection<EmpresaListItemDto> Empresas { get; } = new();

    public string Pesquisa { get => _pesquisa; set => SetProperty(ref _pesquisa, value); }
    public UtilizadorDto? Selecionado { get => _selecionado; set => SetProperty(ref _selecionado, value); }
    public string NomeCompleto { get => _nomeCompleto; set => SetProperty(ref _nomeCompleto, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string NovaPassword { get => _novaPassword; set => SetProperty(ref _novaPassword, value); }
    public int PerfilId { get => _perfilId; set => SetProperty(ref _perfilId, value); }
    public int EmpresaId { get => _empresaId; set => SetProperty(ref _empresaId, value); }
    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }

    public ICommand PesquisarCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand AlternarAtivoCommand { get; }

    public UtilizadoresViewModel(IUtilizadorService service, IPerfilService perfilService, IEmpresaService empresaService)
    {
        _service = service;
        _perfilService = perfilService;
        _empresaService = empresaService;
        PesquisarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        NovoCommand = new RelayCommand(_ => Limpar());
        EditarCommand = new RelayCommand(_ => Editar(), _ => Selecionado is not null);
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => !AGuardar);
        AlternarAtivoCommand = new AsyncRelayCommand(AlternarAtivoAsync);
        _ = InicializarAsync();
    }

    private async Task InicializarAsync()
    {
        var perfis = await _perfilService.ListarAsync();
        Perfis.Clear(); foreach (var item in perfis.Where(x => x.Ativo)) Perfis.Add(item);
        var empresas = await _empresaService.ListarAsync();
        Empresas.Clear(); foreach (var item in empresas.Where(x => x.Ativo)) Empresas.Add(item);
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var lista = await _service.ListarAsync(Pesquisa);
        Utilizadores.Clear(); foreach (var item in lista) Utilizadores.Add(item);
    }

    private void Editar()
    {
        if (Selecionado is null) return;
        _idEdicao = Selecionado.Id;
        NomeCompleto = Selecionado.NomeCompleto;
        Email = Selecionado.Email;
        PerfilId = Selecionado.PerfilId;
        EmpresaId = Selecionado.EmpresaId;
        NovaPassword = string.Empty;
        Mensagem = "Utilizador carregado para edição.";
    }

    private async Task GuardarAsync()
    {
        AGuardar = true; Mensagem = string.Empty;
        try
        {
            await _service.GuardarAsync(new UtilizadorDto
            {
                Id = _idEdicao,
                NomeCompleto = NomeCompleto,
                Email = Email,
                NovaPassword = NovaPassword,
                PerfilId = PerfilId,
                EmpresaId = EmpresaId,
                Ativo = true
            });
            Limpar();
            Mensagem = "Utilizador guardado com sucesso.";
            await CarregarAsync();
        }
        catch (Exception ex) { Mensagem = ex.Message; }
        finally { AGuardar = false; }
    }

    private async Task AlternarAtivoAsync(object? parametro)
    {
        if (parametro is not UtilizadorDto item) return;
        await _service.AlternarAtivoAsync(item.Id, !item.Ativo);
        await CarregarAsync();
    }

    private void Limpar()
    {
        _idEdicao = 0;
        NomeCompleto = Email = NovaPassword = Mensagem = string.Empty;
        PerfilId = Perfis.FirstOrDefault()?.Id ?? 0;
        EmpresaId = Empresas.FirstOrDefault()?.Id ?? 0;
    }
}
