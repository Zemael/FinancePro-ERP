using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Application.Administration.Users;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class UtilizadoresViewModel : ViewModelBase
{
    private readonly UserAdministrationService _service;
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
    private byte[]? _fotoPerfil;
    private string _mensagem = string.Empty;
    private bool _aGuardar;
    private bool _aCarregar;

    public ObservableCollection<UtilizadorDto> Utilizadores { get; } = new();
    public ObservableCollection<PerfilDto> Perfis { get; } = new();
    public ObservableCollection<EmpresaListItemDto> Empresas { get; } = new();

    public string Pesquisa { get => _pesquisa; set => SetProperty(ref _pesquisa, value); }
    public UtilizadorDto? Selecionado { get => _selecionado; set => SetProperty(ref _selecionado, value); }
    public string NomeCompleto { get => _nomeCompleto; set { if (SetProperty(ref _nomeCompleto, value)) OnPropertyChanged(nameof(InicialUtilizador)); } }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string NovaPassword { get => _novaPassword; set => SetProperty(ref _novaPassword, value); }
    public int PerfilId { get => _perfilId; set => SetProperty(ref _perfilId, value); }
    public int EmpresaId { get => _empresaId; set => SetProperty(ref _empresaId, value); }
    public byte[]? FotoPerfil { get => _fotoPerfil; set { if (SetProperty(ref _fotoPerfil, value)) OnPropertyChanged(nameof(InicialUtilizador)); } }
    public string InicialUtilizador => string.IsNullOrWhiteSpace(NomeCompleto) ? "?" : NomeCompleto.Trim()[0].ToString().ToUpperInvariant();
    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }
    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }

    public ICommand PesquisarCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand AlternarAtivoCommand { get; }
    public ICommand RemoverFotoCommand { get; }

    public event Action<int, byte[]?>? FotoPerfilAtualizada;

    public UtilizadoresViewModel(
        UserAdministrationService service,
        IPerfilService perfilService,
        IEmpresaService empresaService)
    {
        _service = service;
        _perfilService = perfilService;
        _empresaService = empresaService;
        PesquisarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        NovoCommand = new RelayCommand(_ => Limpar());
        EditarCommand = new RelayCommand(_ => Editar(), _ => Selecionado is not null);
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => !AGuardar);
        AlternarAtivoCommand = new AsyncRelayCommand(AlternarAtivoAsync);
        RemoverFotoCommand = new RelayCommand(_ => FotoPerfil = null);
        _ = InicializarAsync();
    }

    private async Task InicializarAsync()
    {
        try
        {
            var perfis = await _perfilService.ListarAsync();
            Perfis.Clear();
            foreach (var item in perfis.Where(x => x.Ativo)) Perfis.Add(item);

            var empresas = await _empresaService.ListarAsync();
            Empresas.Clear();
            foreach (var item in empresas.Where(x => x.Ativo)) Empresas.Add(item);

            Limpar();
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            Mensagem = ex.Message;
        }
    }

    private async Task CarregarAsync()
    {
        ACarregar = true;
        try
        {
            var resultado = await _service.ListAsync(Pesquisa);
            if (resultado.IsFailure)
            {
                Mensagem = ObterErro(resultado.Errors, resultado.Message);
                return;
            }

            Utilizadores.Clear();
            foreach (var item in resultado.Value ?? Array.Empty<UtilizadorDto>()) Utilizadores.Add(item);
            Mensagem = resultado.Message ?? $"{Utilizadores.Count} utilizador(es) carregado(s).";
        }
        finally
        {
            ACarregar = false;
        }
    }

    private void Editar()
    {
        if (Selecionado is null) return;
        _idEdicao = Selecionado.Id;
        NomeCompleto = Selecionado.NomeCompleto;
        FotoPerfil = Selecionado.FotoPerfil;
        Email = Selecionado.Email;
        PerfilId = Selecionado.PerfilId;
        EmpresaId = Selecionado.EmpresaId;
        NovaPassword = string.Empty;
        Mensagem = "Utilizador carregado para edição.";
    }

    private async Task GuardarAsync()
    {
        AGuardar = true;
        Mensagem = string.Empty;
        try
        {
            var resultado = await _service.SaveAsync(new UserSaveRequest(
                _idEdicao,
                NomeCompleto,
                Email,
                PerfilId,
                EmpresaId,
                true,
                NovaPassword,
                FotoPerfil));

            if (resultado.IsFailure)
            {
                Mensagem = ObterErro(resultado.Errors, resultado.Message);
                return;
            }

            var mensagemSucesso = resultado.Message ?? "Utilizador guardado com sucesso.";
            var utilizadorGuardadoId = resultado.Value;
            if (utilizadorGuardadoId == SessaoAtual.UtilizadorId)
                FotoPerfilAtualizada?.Invoke(utilizadorGuardadoId, FotoPerfil);
            Limpar();
            Mensagem = mensagemSucesso;
            await CarregarAsync();
            Mensagem = mensagemSucesso;
        }
        finally
        {
            AGuardar = false;
        }
    }

    private async Task AlternarAtivoAsync(object? parametro)
    {
        if (parametro is not UtilizadorDto item) return;
        var resultado = await _service.SetActiveAsync(item.Id, !item.Ativo);
        Mensagem = resultado.IsSuccess
            ? resultado.Message ?? "Estado atualizado."
            : ObterErro(resultado.Errors, resultado.Message);

        if (resultado.IsSuccess) await CarregarAsync();
    }

    private void Limpar()
    {
        _idEdicao = 0;
        NomeCompleto = Email = NovaPassword = string.Empty;
        FotoPerfil = null;
        PerfilId = Perfis.FirstOrDefault()?.Id ?? 0;
        EmpresaId = Empresas.FirstOrDefault()?.Id ?? 0;
        Selecionado = null;
    }

    private static string ObterErro(IEnumerable<string> erros, string? mensagem) =>
        erros.FirstOrDefault() ?? mensagem ?? "Não foi possível concluir a operação.";

    public void DefinirFotoPerfil(byte[] foto) => FotoPerfil = foto;
}
