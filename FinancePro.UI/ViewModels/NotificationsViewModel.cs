using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class NotificationsViewModel : ViewModelBase
{
    private readonly IDashboardService _dashboardService;
    private readonly int _empresaId;
    private readonly int _utilizadorId;
    private bool _aCarregar;
    private string _mensagem = string.Empty;
    private string _ultimaAtualizacao = string.Empty;
    private int _criticos;
    private int _avisos;
    private int _informacoes;

    public ObservableCollection<AlertaDto> Alertas { get; } = new();

    public bool ACarregar { get => _aCarregar; set => SetProperty(ref _aCarregar, value); }
    public string Mensagem { get => _mensagem; set => SetProperty(ref _mensagem, value); }
    public string UltimaAtualizacao { get => _ultimaAtualizacao; set => SetProperty(ref _ultimaAtualizacao, value); }
    public int Criticos { get => _criticos; set => SetProperty(ref _criticos, value); }
    public int Avisos { get => _avisos; set => SetProperty(ref _avisos, value); }
    public int Informacoes { get => _informacoes; set => SetProperty(ref _informacoes, value); }
    public int Total => Criticos + Avisos + Informacoes;

    public ICommand AtualizarCommand { get; }

    public NotificationsViewModel(IDashboardService dashboardService, int empresaId, int utilizadorId)
    {
        _dashboardService = dashboardService;
        _empresaId = empresaId;
        _utilizadorId = utilizadorId;
        AtualizarCommand = new AsyncRelayCommand(_ => CarregarAsync());
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        ACarregar = true;
        Mensagem = string.Empty;
        try
        {
            var resumo = await _dashboardService.ObterResumoAsync(_empresaId, _utilizadorId);
            var ordenados = resumo.Alertas
                .OrderBy(a => Ordem(a.Severidade))
                .ThenBy(a => a.Mensagem)
                .ToList();

            Alertas.Clear();
            foreach (var alerta in ordenados) Alertas.Add(alerta);

            Criticos = ordenados.Count(a => string.Equals(a.Severidade, "Critico", StringComparison.OrdinalIgnoreCase));
            Avisos = ordenados.Count(a => string.Equals(a.Severidade, "Aviso", StringComparison.OrdinalIgnoreCase));
            Informacoes = ordenados.Count - Criticos - Avisos;
            OnPropertyChanged(nameof(Total));
            UltimaAtualizacao = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            Mensagem = Total == 0 ? "Nenhum alerta ativo." : $"{Total} alerta(s) ativo(s).";
        }
        catch (Exception ex)
        {
            Mensagem = $"Não foi possível carregar os alertas: {ex.Message}";
        }
        finally
        {
            ACarregar = false;
        }
    }

    private static int Ordem(string? severidade) => severidade?.ToLowerInvariant() switch
    {
        "critico" => 0,
        "aviso" => 1,
        _ => 2
    };
}
