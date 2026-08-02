using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public class ConfiguracoesViewModel : ViewModelBase
{
    private readonly IConfiguracoesService _service;
    private readonly int _empresaId;

    private string _empresaNome = string.Empty;
    private string _moeda = string.Empty;
    private string _nif = string.Empty;
    private string _morada = string.Empty;
    private string _telefone = string.Empty;
    private string _emailEmpresa = string.Empty;
    private string _mensagemErroEmpresa = string.Empty;
    private bool _aGuardarEmpresa;

    private string _novoNome = string.Empty;
    private string _novoEmail = string.Empty;
    private string _novaPassword = string.Empty;
    private PerfilOpcaoDto? _perfilSelecionado;
    private string _mensagemErroUtilizador = string.Empty;
    private bool _aGuardarUtilizador;

    public string EmpresaNome { get => _empresaNome; set => SetProperty(ref _empresaNome, value); }
    public string Moeda { get => _moeda; set => SetProperty(ref _moeda, value); }
    public string NIF { get => _nif; set => SetProperty(ref _nif, value); }
    public string Morada { get => _morada; set => SetProperty(ref _morada, value); }
    public string Telefone { get => _telefone; set => SetProperty(ref _telefone, value); }
    public string EmailEmpresa { get => _emailEmpresa; set => SetProperty(ref _emailEmpresa, value); }
    public string MensagemErroEmpresa { get => _mensagemErroEmpresa; set => SetProperty(ref _mensagemErroEmpresa, value); }
    public bool AGuardarEmpresa { get => _aGuardarEmpresa; set => SetProperty(ref _aGuardarEmpresa, value); }
    public ICommand GuardarEmpresaCommand { get; }

    public string NovoNome { get => _novoNome; set => SetProperty(ref _novoNome, value); }
    public string NovoEmail { get => _novoEmail; set => SetProperty(ref _novoEmail, value); }
    public string NovaPassword { get => _novaPassword; set => SetProperty(ref _novaPassword, value); }
    public PerfilOpcaoDto? PerfilSelecionado { get => _perfilSelecionado; set => SetProperty(ref _perfilSelecionado, value); }
    public string MensagemErroUtilizador { get => _mensagemErroUtilizador; set => SetProperty(ref _mensagemErroUtilizador, value); }
    public bool AGuardarUtilizador { get => _aGuardarUtilizador; set => SetProperty(ref _aGuardarUtilizador, value); }

    public ObservableCollection<PerfilOpcaoDto> Perfis { get; } = new();
    public ObservableCollection<UtilizadorListItemDto> Utilizadores { get; } = new();

    public ICommand CriarUtilizadorCommand { get; }
    public ICommand AlternarAtivoUtilizadorCommand { get; }

    public ConfiguracoesViewModel(IConfiguracoesService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;

        GuardarEmpresaCommand = new AsyncRelayCommand(_ => GuardarEmpresaAsync(), _ => !AGuardarEmpresa);
        CriarUtilizadorCommand = new AsyncRelayCommand(_ => CriarUtilizadorAsync(), _ => !AGuardarUtilizador);
        AlternarAtivoUtilizadorCommand = new AsyncRelayCommand(AlternarAtivoUtilizadorAsync);

        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var empresa = await _service.ObterEmpresaAsync(_empresaId);
        if (empresa is not null)
        {
            EmpresaNome = empresa.Nome;
            Moeda = empresa.Moeda;
            NIF = empresa.NIF ?? string.Empty;
            Morada = empresa.Morada ?? string.Empty;
            Telefone = empresa.Telefone ?? string.Empty;
            EmailEmpresa = empresa.Email ?? string.Empty;
        }

        var perfis = await _service.ListarPerfisAsync();
        Perfis.Clear();
        foreach (var perfil in perfis)
        {
            Perfis.Add(perfil);
        }
        PerfilSelecionado = Perfis.FirstOrDefault();

        await CarregarUtilizadoresAsync();
    }

    private async Task CarregarUtilizadoresAsync()
    {
        var utilizadores = await _service.ListarUtilizadoresAsync(_empresaId);
        Utilizadores.Clear();
        foreach (var utilizador in utilizadores)
        {
            Utilizadores.Add(utilizador);
        }
    }

    private async Task GuardarEmpresaAsync()
    {
        MensagemErroEmpresa = string.Empty;

        if (string.IsNullOrWhiteSpace(EmpresaNome))
        {
            MensagemErroEmpresa = "Indique o nome da empresa.";
            return;
        }

        AGuardarEmpresa = true;
        try
        {
            await _service.AtualizarEmpresaAsync(new EmpresaDto
            {
                Id = _empresaId,
                Nome = EmpresaNome,
                Moeda = Moeda,
                NIF = NIF,
                Morada = Morada,
                Telefone = Telefone,
                Email = EmailEmpresa
            });
        }
        catch (Exception ex)
        {
            MensagemErroEmpresa = ex.Message;
        }
        finally
        {
            AGuardarEmpresa = false;
        }
    }

    private async Task CriarUtilizadorAsync()
    {
        MensagemErroUtilizador = string.Empty;

        if (PerfilSelecionado is null)
        {
            MensagemErroUtilizador = "Selecione o perfil.";
            return;
        }

        AGuardarUtilizador = true;
        try
        {
            await _service.CriarUtilizadorAsync(new NovoUtilizadorDto
            {
                NomeCompleto = NovoNome,
                Email = NovoEmail,
                Password = NovaPassword,
                PerfilId = PerfilSelecionado.Id,
                EmpresaId = _empresaId
            });

            NovoNome = string.Empty;
            NovoEmail = string.Empty;
            NovaPassword = string.Empty;

            await CarregarUtilizadoresAsync();
        }
        catch (Exception ex)
        {
            MensagemErroUtilizador = ex.Message;
        }
        finally
        {
            AGuardarUtilizador = false;
        }
    }

    private async Task AlternarAtivoUtilizadorAsync(object? parametro)
    {
        if (parametro is not UtilizadorListItemDto utilizador)
        {
            return;
        }

        await _service.AlternarAtivoUtilizadorAsync(utilizador.Id, !utilizador.Ativo);
        await CarregarUtilizadoresAsync();
    }
}
