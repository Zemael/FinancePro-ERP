using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class CaixaViewModel : ViewModelBase
{
    private readonly ICaixaService _service;
    private readonly int _empresaId;
    private readonly int _utilizadorId;
    private List<CaixaListItemDto> _todasAsCaixas = new();

    private string _filtro = string.Empty;
    private CaixaListItemDto? _caixaSelecionada;
    private bool _emEdicao;
    private int? _idEmEdicao;
    private string _nome = string.Empty;
    private string _saldoInicialTexto = string.Empty;
    private string _saldoMinimoTexto = string.Empty;
    private bool _permiteSaldoNegativo;
    private string _mensagemErro = string.Empty;
    private string _mensagemInfo = string.Empty;
    private bool _aGuardar;

    private SessaoCaixaDto? _sessaoAtual;
    private string _saldoAberturaTexto = "0";
    private string _saldoContadoTexto = "0";
    private string _observacaoSessao = string.Empty;

    public string Filtro { get => _filtro; set { if (SetProperty(ref _filtro, value)) AplicarFiltro(); } }
    public CaixaListItemDto? CaixaSelecionada
    {
        get => _caixaSelecionada;
        set
        {
            if (!SetProperty(ref _caixaSelecionada, value)) return;
            _ = CarregarSessaoAsync();
            CommandManager.InvalidateRequerySuggested();
        }
    }
    public bool EmEdicao { get => _emEdicao; set => SetProperty(ref _emEdicao, value); }
    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string SaldoInicialTexto { get => _saldoInicialTexto; set => SetProperty(ref _saldoInicialTexto, value); }
    public string SaldoMinimoTexto { get => _saldoMinimoTexto; set => SetProperty(ref _saldoMinimoTexto, value); }
    public bool PermiteSaldoNegativo { get => _permiteSaldoNegativo; set => SetProperty(ref _permiteSaldoNegativo, value); }
    public string MensagemErro { get => _mensagemErro; set => SetProperty(ref _mensagemErro, value); }
    public string MensagemInfo { get => _mensagemInfo; set => SetProperty(ref _mensagemInfo, value); }
    public bool AGuardar { get => _aGuardar; set => SetProperty(ref _aGuardar, value); }

    public SessaoCaixaDto? SessaoAtual { get => _sessaoAtual; private set { if (SetProperty(ref _sessaoAtual, value)) { OnPropertyChanged(nameof(TemSessaoAberta)); OnPropertyChanged(nameof(EstadoSessaoTexto)); CommandManager.InvalidateRequerySuggested(); } } }
    public bool TemSessaoAberta => SessaoAtual?.Aberta == true;
    public string EstadoSessaoTexto => TemSessaoAberta ? "Aberto" : "Fechado";
    public string SaldoAberturaTexto { get => _saldoAberturaTexto; set => SetProperty(ref _saldoAberturaTexto, value); }
    public string SaldoContadoTexto { get => _saldoContadoTexto; set => SetProperty(ref _saldoContadoTexto, value); }
    public string ObservacaoSessao { get => _observacaoSessao; set => SetProperty(ref _observacaoSessao, value); }

    public ObservableCollection<CaixaListItemDto> Caixas { get; } = new();

    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand EliminarCommand { get; }
    public ICommand ExportarCommand { get; }
    public ICommand AbrirCaixaCommand { get; }
    public ICommand FecharCaixaCommand { get; }
    public ICommand AtualizarSessaoCommand { get; }

    public CaixaViewModel(ICaixaService service, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;
        _utilizadorId = FinancePro.UI.Common.SessaoAtual.UtilizadorId;

        NovoCommand = new AsyncRelayCommand(_ => { IniciarNovo(); return Task.CompletedTask; });
        EditarCommand = new AsyncRelayCommand(_ => { IniciarEdicao(); return Task.CompletedTask; });
        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync(), _ => !AGuardar);
        EliminarCommand = new AsyncRelayCommand(_ => EliminarAsync());
        ExportarCommand = new AsyncRelayCommand(_ => { Exportar(); return Task.CompletedTask; });
        AbrirCaixaCommand = new AsyncRelayCommand(_ => AbrirCaixaAsync(), _ => !AGuardar && !TemSessaoAberta);
        FecharCaixaCommand = new AsyncRelayCommand(_ => FecharCaixaAsync(), _ => !AGuardar && TemSessaoAberta);
        AtualizarSessaoCommand = new AsyncRelayCommand(_ => CarregarSessaoAsync());

        _ = InicializarAsync();
    }

    private async Task InicializarAsync()
    {
        await CarregarAsync();
        if (CaixaSelecionada is not null) await CarregarSessaoAsync();
    }

    private async Task CarregarAsync()
    {
        _todasAsCaixas = (await _service.ListarAsync(_empresaId)).ToList();
        AplicarFiltro();
    }

    private async Task CarregarSessaoAsync()
    {
        var caixaId = CaixaSelecionada?.Id;
        var sessao = caixaId.HasValue
            ? await _service.ObterSessaoAbertaAsync(_empresaId, caixaId.Value)
            : null;
        if (CaixaSelecionada?.Id != caixaId) return;
        SessaoAtual = sessao;
        if (SessaoAtual is not null)
            SaldoContadoTexto = SessaoAtual.SaldoAtual.ToString("0.00");
        else
            SaldoContadoTexto = "0";
    }

    private void AplicarFiltro()
    {
        var termo = Filtro.Trim();
        var filtradas = string.IsNullOrEmpty(termo)
            ? _todasAsCaixas
            : _todasAsCaixas.Where(c => c.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase)).ToList();
        Caixas.Clear();
        foreach (var caixa in filtradas) Caixas.Add(caixa);
        if (CaixaSelecionada is null || !Caixas.Contains(CaixaSelecionada))
            CaixaSelecionada = Caixas.FirstOrDefault(c => c.Ativo);
    }

    private async Task AbrirCaixaAsync()
    {
        MensagemErro = string.Empty;
        MensagemInfo = string.Empty;
        if (CaixaSelecionada is null) { MensagemErro = "Selecione a caixa que pretende abrir."; return; }
        if (!CaixaSelecionada.Ativo) { MensagemErro = "A caixa selecionada está inativa."; return; }
        if (_utilizadorId <= 0) { MensagemErro = "A sessão do utilizador expirou. Termine a sessão e entre novamente."; return; }
        if (!decimal.TryParse(SaldoAberturaTexto, out var saldo) || saldo < 0) { MensagemErro = "Indique um saldo inicial válido."; return; }
        AGuardar = true;
        try
        {
            SessaoAtual = await _service.AbrirAsync(new AbrirCaixaDto
            {
                CaixaId = CaixaSelecionada.Id,
                EmpresaId = _empresaId,
                UtilizadorId = _utilizadorId,
                SaldoInicial = saldo,
                Observacao = ObservacaoSessao
            });
            SaldoContadoTexto = SessaoAtual.SaldoAtual.ToString("0.00");
            MensagemInfo = $"Caixa {SessaoAtual.CaixaNome} aberto com sucesso.";
            ObservacaoSessao = string.Empty;
        }
        catch (Exception ex) { MensagemErro = ex.Message; }
        finally { AGuardar = false; }
    }

    private async Task FecharCaixaAsync()
    {
        MensagemErro = string.Empty;
        MensagemInfo = string.Empty;
        if (SessaoAtual is null) { MensagemErro = "Não existe sessão aberta."; return; }
        if (!decimal.TryParse(SaldoContadoTexto, out var saldoContado) || saldoContado < 0) { MensagemErro = "Indique o saldo contado no fecho."; return; }
        AGuardar = true;
        try
        {
            var sessaoFechada = await _service.FecharAsync(new FecharCaixaDto
            {
                SessaoCaixaId = SessaoAtual.Id,
                SaldoContado = saldoContado,
                Observacao = ObservacaoSessao
            });
            MensagemInfo = $"Caixa {sessaoFechada.CaixaNome} fechado. Saldo calculado: {sessaoFechada.SaldoAtual:N2}.";
            SessaoAtual = null;
            SaldoAberturaTexto = "0";
            SaldoContadoTexto = "0";
            ObservacaoSessao = string.Empty;
        }
        catch (Exception ex) { MensagemErro = ex.Message; }
        finally { AGuardar = false; }
    }

    private void IniciarNovo()
    {
        MensagemErro = string.Empty; MensagemInfo = string.Empty; _idEmEdicao = null;
        Nome = string.Empty; SaldoInicialTexto = string.Empty; SaldoMinimoTexto = string.Empty;
        PermiteSaldoNegativo = false; EmEdicao = true;
    }

    private void IniciarEdicao()
    {
        if (CaixaSelecionada is null) { MensagemErro = "Selecione uma caixa na lista para editar."; return; }
        MensagemErro = string.Empty; MensagemInfo = string.Empty; _idEmEdicao = CaixaSelecionada.Id;
        Nome = CaixaSelecionada.Nome; SaldoInicialTexto = CaixaSelecionada.SaldoInicial.ToString();
        SaldoMinimoTexto = CaixaSelecionada.SaldoMinimo?.ToString() ?? string.Empty;
        PermiteSaldoNegativo = CaixaSelecionada.PermiteSaldoNegativo; EmEdicao = true;
    }

    private async Task GuardarAsync()
    {
        MensagemErro = string.Empty;
        if (string.IsNullOrWhiteSpace(Nome)) { MensagemErro = "Indique o nome da caixa."; return; }
        if (!decimal.TryParse(SaldoInicialTexto, out var saldoInicial) || saldoInicial < 0) saldoInicial = 0;
        decimal? saldoMinimo = decimal.TryParse(SaldoMinimoTexto, out var sm) ? sm : null;
        var dto = new NovoCaixaDto { Nome = Nome, SaldoInicial = saldoInicial, SaldoMinimo = saldoMinimo, PermiteSaldoNegativo = PermiteSaldoNegativo, EmpresaId = _empresaId };
        AGuardar = true;
        try
        {
            if (_idEmEdicao.HasValue) { await _service.AtualizarAsync(_idEmEdicao.Value, dto); MensagemInfo = "Caixa atualizada."; }
            else { await _service.CriarAsync(dto); MensagemInfo = "Caixa criada."; }
            EmEdicao = false; await CarregarAsync();
        }
        catch (Exception ex) { MensagemErro = ex.Message; }
        finally { AGuardar = false; }
    }

    private async Task EliminarAsync()
    {
        MensagemErro = string.Empty;
        if (CaixaSelecionada is null) { MensagemErro = "Selecione uma caixa na lista para desativar."; return; }
        try
        {
            await _service.AlternarAtivoAsync(CaixaSelecionada.Id, false);
            MensagemInfo = "Caixa desativada.";
            await CarregarAsync();
        }
        catch (Exception ex) { MensagemErro = ex.Message; }
    }

    private void Exportar()
    {
        var cabecalhos = new[] { "Nome", "Saldo Inicial", "Saldo Mínimo", "Permite Saldo Negativo", "Estado" };
        var linhas = Caixas.Select(c => (IReadOnlyList<string>)new[] { c.Nome, c.SaldoInicial.ToString("0.00"), c.SaldoMinimo?.ToString("0.00") ?? "", c.PermiteSaldoNegativo ? "Sim" : "Não", c.Ativo ? "Ativo" : "Inativo" });
        ExportadorCsv.Exportar("caixas.csv", cabecalhos, linhas);
        MensagemInfo = "Ficheiro caixas.csv exportado.";
    }
}
