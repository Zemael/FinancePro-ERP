using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class PermissoesViewModel : ViewModelBase
{
    private readonly IPermissaoService _permissaoService;
    private readonly IPerfilService _perfilService;
    private PerfilDto? _perfilSelecionado;
    private string _mensagem = "Selecione um perfil para configurar as permissões.";
    private bool _aGuardar;

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

    public ICommand GuardarCommand { get; }
    public ICommand MarcarTudoCommand { get; }
    public ICommand LimparTudoCommand { get; }
    public ICommand RecarregarCommand { get; }

    public PermissoesViewModel(IPermissaoService permissaoService, IPerfilService perfilService)
    {
        _permissaoService = permissaoService;
        _perfilService = perfilService;
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => PerfilSelecionado is not null && !AGuardar);
        RecarregarCommand = new AsyncRelayCommand(_ => CarregarMatrizAsync(), _ => PerfilSelecionado is not null);
        MarcarTudoCommand = new RelayCommand(_ => DefinirTudo(true), _ => PerfilSelecionado is not null);
        LimparTudoCommand = new RelayCommand(_ => DefinirTudo(false), _ => PerfilSelecionado is not null);
        _ = CarregarPerfisAsync();
    }

    private async Task CarregarPerfisAsync()
    {
        var lista = await _perfilService.ListarAsync();
        Perfis.Clear();
        foreach (var perfil in lista.Where(x => x.Ativo)) Perfis.Add(perfil);
        PerfilSelecionado = Perfis.FirstOrDefault();
    }

    private async Task CarregarMatrizAsync()
    {
        if (PerfilSelecionado is null) return;
        var matriz = await _permissaoService.ObterMatrizAsync(PerfilSelecionado.Id);
        Permissoes.Clear();
        foreach (var item in matriz) Permissoes.Add(item);
        Mensagem = $"Permissões do perfil {PerfilSelecionado.Nome}.";
    }

    private async Task GuardarAsync()
    {
        if (PerfilSelecionado is null) return;
        AGuardar = true;
        try
        {
            await _permissaoService.GuardarMatrizAsync(PerfilSelecionado.Id, Permissoes.ToList());
            Mensagem = "Permissões guardadas. Produzirão efeito no próximo login do utilizador.";
        }
        catch (Exception ex) { Mensagem = ex.Message; }
        finally { AGuardar = false; }
    }

    private void DefinirTudo(bool valor)
    {
        foreach (var p in Permissoes)
        {
            p.Consultar = valor; p.Criar = valor; p.Editar = valor; p.Desativar = valor;
            p.Aprovar = valor; p.Exportar = valor; p.Administrar = valor;
        }
        var copia = Permissoes.ToList();
        Permissoes.Clear();
        foreach (var p in copia) Permissoes.Add(p);
        Mensagem = valor ? "Todas as permissões foram marcadas." : "Todas as permissões foram removidas.";
    }
}
