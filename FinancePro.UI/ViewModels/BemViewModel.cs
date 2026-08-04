using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public record CampoPropostoInfo(string Campo, string Tipo, string Obrigatorio);

/// <summary>
/// Módulo 08 — Gestão Patrimonial. Primeiro ViewModel construído com
/// CommunityToolkit.Mvvm (source generators [ObservableProperty]/
/// [RelayCommand]) em vez do ViewModelBase manual usado nos módulos
/// anteriores.
/// </summary>
public partial class BemViewModel : ObservableObject
{
    private readonly IBemService _service;
    private readonly IAuditoriaService _auditoria;
    private readonly int _empresaId;

    private List<BemListItemDto> _todosOsBens = new();
    private int? _idEmEdicao;

    [ObservableProperty] private string filtro = string.Empty;
    [ObservableProperty] private BemListItemDto? bemSelecionado;
    [ObservableProperty] private bool emEdicao;

    [ObservableProperty] private string numeroPatrimonial = string.Empty;
    [ObservableProperty] private string descricao = string.Empty;
    [ObservableProperty] private string categoria = string.Empty;
    [ObservableProperty] private string marca = string.Empty;
    [ObservableProperty] private string modelo = string.Empty;
    [ObservableProperty] private string serie = string.Empty;
    [ObservableProperty] private string localizacao = string.Empty;
    [ObservableProperty] private string responsavel = string.Empty;
    [ObservableProperty] private DateTime dataAquisicao = DateTime.Today;
    [ObservableProperty] private string valorAquisicaoTexto = string.Empty;
    [ObservableProperty] private string vidaUtilAnosTexto = string.Empty;
    [ObservableProperty] private MetodoDepreciacao metodoDepreciacaoSelecionado = MetodoDepreciacao.Linear;

    [ObservableProperty] private string mensagemErro = string.Empty;
    [ObservableProperty] private string mensagemInfo = string.Empty;
    [ObservableProperty] private bool aGuardar;
    [ObservableProperty] private string ultimaAtualizacao = "--";

    public IReadOnlyList<MetodoDepreciacao> MetodosDisponiveis { get; } = Enum.GetValues<MetodoDepreciacao>().ToList();

    public IReadOnlyList<CampoPropostoInfo> CamposPropostos { get; } = new List<CampoPropostoInfo>
    {
        new("Código", "NVARCHAR(20)", "Sim (gerado automaticamente)"),
        new("Número Patrimonial", "NVARCHAR(30)", "Sim"),
        new("Descrição", "NVARCHAR(200)", "Sim"),
        new("Categoria", "NVARCHAR(100)", "Conforme regra"),
        new("Marca", "NVARCHAR(100)", "Conforme regra"),
        new("Modelo", "NVARCHAR(100)", "Conforme regra"),
        new("Série", "NVARCHAR(100)", "Conforme regra"),
        new("Localização", "NVARCHAR(150)", "Conforme regra"),
        new("Responsável", "NVARCHAR(150)", "Conforme regra"),
        new("Data de Aquisição", "DATE", "Sim"),
        new("Valor de Aquisição", "DECIMAL(18,2)", "Sim"),
        new("Vida Útil (anos)", "INT", "Conforme regra"),
        new("Método de Depreciação", "NVARCHAR(20)", "Conforme regra"),
        new("Estado", "NVARCHAR(20)", "Conforme regra"),
    };

    /// <summary>Permissão: só Administrador/Gestor pode eliminar/abater um bem.</summary>
    public bool PodeEliminar => SessaoAtual.PodeEliminarOuAprovar;

    public ObservableCollection<BemListItemDto> Bens { get; } = new();
    public ObservableCollection<LogAuditoriaDto> HistoricoAuditoria { get; } = new();
    public int TotalBens => _todosOsBens.Count;
    public int BensAtivos => _todosOsBens.Count(b => !b.Estado.Contains("Abat", StringComparison.OrdinalIgnoreCase));
    public decimal ValorPatrimonio => _todosOsBens.Sum(b => b.ValorAquisicao);
    public decimal ValorLiquido => _todosOsBens.Sum(b => b.ValorLiquidoAtual);

    public BemViewModel(IBemService service, IAuditoriaService auditoria, int empresaId)
    {
        _service = service;
        _auditoria = auditoria;
        _empresaId = empresaId;

        _ = CarregarAsync();
    }

    partial void OnFiltroChanged(string value) => AplicarFiltro();

    partial void OnBemSelecionadoChanged(BemListItemDto? value)
    {
        if (value is not null)
        {
            _ = CarregarHistoricoAsync(value.Id);
        }
        else
        {
            HistoricoAuditoria.Clear();
        }
    }

    private async Task CarregarAsync()
    {
        _todosOsBens = (await _service.ListarAsync(_empresaId)).ToList();
        UltimaAtualizacao = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        var termo = Filtro.Trim();
        var filtrados = string.IsNullOrEmpty(termo)
            ? _todosOsBens
            : _todosOsBens.Where(b =>
                b.Descricao.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                b.NumeroPatrimonial.Contains(termo, StringComparison.OrdinalIgnoreCase)).ToList();

        Bens.Clear();
        foreach (var bem in filtrados)
        {
            Bens.Add(bem);
        }
        OnPropertyChanged(nameof(TotalBens));
        OnPropertyChanged(nameof(ValorPatrimonio));
        OnPropertyChanged(nameof(ValorLiquido));
        OnPropertyChanged(nameof(BensAtivos));
    }

    private async Task CarregarHistoricoAsync(int bemId)
    {
        var historico = await _auditoria.ListarPorRegistoAsync("Bem", bemId);
        HistoricoAuditoria.Clear();
        foreach (var entrada in historico)
        {
            HistoricoAuditoria.Add(entrada);
        }
    }

    [RelayCommand]
    private Task AtualizarAsync() => CarregarAsync();

    [RelayCommand]
    private void Novo()
    {
        MensagemErro = string.Empty;
        MensagemInfo = string.Empty;
        _idEmEdicao = null;
        NumeroPatrimonial = string.Empty;
        Descricao = string.Empty;
        Categoria = string.Empty;
        Marca = string.Empty;
        Modelo = string.Empty;
        Serie = string.Empty;
        Localizacao = string.Empty;
        Responsavel = string.Empty;
        DataAquisicao = DateTime.Today;
        ValorAquisicaoTexto = string.Empty;
        VidaUtilAnosTexto = string.Empty;
        MetodoDepreciacaoSelecionado = MetodoDepreciacao.Linear;
        EmEdicao = true;
    }

    [RelayCommand]
    private void Editar()
    {
        if (BemSelecionado is null)
        {
            MensagemErro = "Selecione um bem na lista para editar.";
            return;
        }

        MensagemErro = string.Empty;
        MensagemInfo = string.Empty;
        _idEmEdicao = BemSelecionado.Id;
        NumeroPatrimonial = BemSelecionado.NumeroPatrimonial;
        Descricao = BemSelecionado.Descricao;
        Categoria = BemSelecionado.Categoria ?? string.Empty;
        Marca = BemSelecionado.Marca ?? string.Empty;
        Modelo = BemSelecionado.Modelo ?? string.Empty;
        Serie = BemSelecionado.Serie ?? string.Empty;
        Localizacao = BemSelecionado.Localizacao ?? string.Empty;
        Responsavel = BemSelecionado.Responsavel ?? string.Empty;
        DataAquisicao = BemSelecionado.DataAquisicao;
        ValorAquisicaoTexto = BemSelecionado.ValorAquisicao.ToString();
        VidaUtilAnosTexto = BemSelecionado.VidaUtilAnos.ToString();
        MetodoDepreciacaoSelecionado = Enum.TryParse<MetodoDepreciacao>(BemSelecionado.MetodoDepreciacao, out var m) ? m : MetodoDepreciacao.Linear;
        EmEdicao = true;
    }

    private bool PodeGuardar() => !AGuardar;

    [RelayCommand(CanExecute = nameof(PodeGuardar))]
    private async Task GuardarAsync()
    {
        MensagemErro = string.Empty;

        if (!decimal.TryParse(ValorAquisicaoTexto, out var valorAquisicao) || valorAquisicao < 0)
        {
            MensagemErro = "Indique um valor de aquisição válido.";
            return;
        }

        if (!int.TryParse(VidaUtilAnosTexto, out var vidaUtil) || vidaUtil < 0)
        {
            MensagemErro = "Indique a vida útil em anos (número inteiro).";
            return;
        }

        var dto = new NovoBemDto
        {
            NumeroPatrimonial = NumeroPatrimonial,
            Descricao = Descricao,
            Categoria = Categoria,
            Marca = Marca,
            Modelo = Modelo,
            Serie = Serie,
            Localizacao = Localizacao,
            Responsavel = Responsavel,
            DataAquisicao = DataAquisicao,
            ValorAquisicao = valorAquisicao,
            VidaUtilAnos = vidaUtil,
            MetodoDepreciacao = MetodoDepreciacaoSelecionado,
            EmpresaId = _empresaId
        };

        AGuardar = true;
        try
        {
            if (_idEmEdicao.HasValue)
            {
                await _service.AtualizarAsync(_idEmEdicao.Value, dto, SessaoAtual.UtilizadorId, SessaoAtual.NomeCompleto);
                MensagemInfo = "Bem atualizado.";
            }
            else
            {
                await _service.CriarAsync(dto, SessaoAtual.UtilizadorId, SessaoAtual.NomeCompleto);
                MensagemInfo = "Bem criado.";
            }

            EmEdicao = false;
            await CarregarAsync();
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

    private bool PodeEliminarAgora() => PodeEliminar;

    [RelayCommand(CanExecute = nameof(PodeEliminarAgora))]
    private async Task EliminarAsync()
    {
        MensagemErro = string.Empty;

        if (!PodeEliminar)
        {
            MensagemErro = "Só Administrador ou Gestor pode abater/eliminar um bem.";
            return;
        }

        if (BemSelecionado is null)
        {
            MensagemErro = "Selecione um bem na lista para eliminar.";
            return;
        }

        await _service.EliminarAsync(BemSelecionado.Id, SessaoAtual.UtilizadorId, SessaoAtual.NomeCompleto);
        MensagemInfo = "Bem abatido/desativado (histórico preservado).";
        await CarregarAsync();
    }

    [RelayCommand]
    private void Exportar()
    {
        var cabecalhos = new[] { "Código", "Nº Patrimonial", "Descrição", "Categoria", "Responsável", "Data Aquisição", "Valor Aquisição", "Valor Líquido Atual", "Estado" };
        var linhas = Bens.Select(b => (IReadOnlyList<string>)new[]
        {
            b.Codigo, b.NumeroPatrimonial, b.Descricao, b.Categoria ?? "", b.Responsavel ?? "",
            b.DataAquisicao.ToString("dd/MM/yyyy"), b.ValorAquisicao.ToString("0.00"), b.ValorLiquidoAtual.ToString("0.00"), b.Estado
        });

        if (ExportadorCsv.Exportar("Bens.csv", cabecalhos, linhas))
        {
            MensagemInfo = "Exportado com sucesso.";
        }
    }
}
