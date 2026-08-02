using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Application.Administration.Permissions;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class PermissoesViewModel : ViewModelBase
{
    private readonly PermissionAdministrationService _service;
    private readonly IPerfilService _perfilService;
    private PerfilDto? _perfilSelecionado;
    private string _mensagem = "Selecione um perfil para configurar as permissões.";
    private bool _aGuardar;
    private bool _aCarregar;

    public ObservableCollection<PerfilDto> Perfis { get; } = new();
    public ObservableCollection<PermissaoPerfilDto> Permissoes { get; } = new();

    public PerfilDto? PerfilSelecionado
    {
        get => _perfilSelecionado;
        set
        {
            if (SetProperty(ref _perfilSelecionado, value) && value is not null)
                _ = CarregarMatrizAsync();
        }
    }

    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }
    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }

    public ICommand GuardarCommand { get; }
    public ICommand MarcarTudoCommand { get; }
    public ICommand LimparTudoCommand { get; }
    public ICommand RecarregarCommand { get; }

    public PermissoesViewModel(PermissionAdministrationService service, IPerfilService perfilService)
    {
        _service = service;
        _perfilService = perfilService;
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => PerfilSelecionado is not null && !AGuardar);
        RecarregarCommand = new AsyncRelayCommand(_ => CarregarMatrizAsync(), _ => PerfilSelecionado is not null);
        MarcarTudoCommand = new RelayCommand(_ => DefinirTudo(true), _ => PerfilSelecionado is not null);
        LimparTudoCommand = new RelayCommand(_ => DefinirTudo(false), _ => PerfilSelecionado is not null);
        _ = CarregarPerfisAsync();
    }

    private async Task CarregarPerfisAsync()
    {
        try
        {
            var lista = await _perfilService.ListarAsync();
            Perfis.Clear();
            foreach (var perfil in lista.Where(x => x.Ativo)) Perfis.Add(perfil);
            PerfilSelecionado = Perfis.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Mensagem = ex.Message;
        }
    }

    private async Task CarregarMatrizAsync()
    {
        if (PerfilSelecionado is null) return;
        ACarregar = true;
        try
        {
            var resultado = await _service.GetMatrixAsync(PerfilSelecionado.Id);
            if (resultado.IsFailure)
            {
                Mensagem = ObterErro(resultado.Errors, resultado.Message);
                return;
            }

            Permissoes.Clear();
            foreach (var item in resultado.Value ?? Array.Empty<PermissaoPerfilDto>()) Permissoes.Add(item);
            Mensagem = $"Permissões do perfil {PerfilSelecionado.Nome}.";
        }
        finally
        {
            ACarregar = false;
        }
    }

    private async Task GuardarAsync()
    {
        if (PerfilSelecionado is null) return;
        AGuardar = true;
        try
        {
            var resultado = await _service.SaveMatrixAsync(PerfilSelecionado.Id, Permissoes.ToList());
            Mensagem = resultado.IsSuccess
                ? resultado.Message ?? "Permissões guardadas com sucesso."
                : ObterErro(resultado.Errors, resultado.Message);
        }
        finally
        {
            AGuardar = false;
        }
    }

    private void DefinirTudo(bool valor)
    {
        foreach (var p in Permissoes)
        {
            p.Consultar = valor;
            p.Criar = valor;
            p.Editar = valor;
            p.Desativar = valor;
            p.Aprovar = valor;
            p.Exportar = valor;
            p.Administrar = valor;
        }

        var copia = Permissoes.ToList();
        Permissoes.Clear();
        foreach (var p in copia) Permissoes.Add(p);
        Mensagem = valor ? "Todas as permissões foram marcadas." : "Todas as permissões foram removidas.";
    }

    private static string ObterErro(IEnumerable<string> erros, string? mensagem) =>
        erros.FirstOrDefault() ?? mensagem ?? "Não foi possível concluir a operação.";
}
