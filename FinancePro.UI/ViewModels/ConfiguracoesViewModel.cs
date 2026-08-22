using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;
using FinancePro.UI.Common;
using FinancePro.Platform.Settings;

namespace FinancePro.UI.ViewModels;

public class ConfiguracoesViewModel : ViewModelBase
{
    private readonly IConfiguracoesService _service;
    private readonly int _empresaId;
    private readonly ISettingsService _settings;

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
    private string _tema = "Light";
    private bool _permitirCaixaNegativo;
    private decimal _limiteAprovacaoPagamento;
    private string _mensagemParametros = string.Empty;
    private string _databaseNome = string.Empty;
    private string _databaseServidor = string.Empty;
    private decimal _databaseTamanhoMb;
    private string _ultimoBackup = "Nunca / indisponível";
    private string _resumoSistema = string.Empty;
    private string _mensagemBackup = string.Empty;
    private bool _backupEmCurso;
    private string _sqlServerVersao = string.Empty;
    private string _databaseEstado = string.Empty;
    private string _recoveryModel = string.Empty;
    private int _compatibilityLevel;
    private string _integridadeBase = "Ainda não verificada";
    private string _mensagemDiagnostico = string.Empty;
    private bool _diagnosticoEmCurso;
    private string _mensagemSuporte = string.Empty;
    private bool _relatorioSuporteEmCurso;
    private string _estadoProntidao = "Ainda não verificado";
    private string _resumoProntidao = string.Empty;
    private bool _prontidaoEmCurso;
    private string _mensagemHistoricoBackups = string.Empty;
    private bool _historicoBackupsEmCurso;
    private int _backupsAPreservar = 5;
    private bool _confirmarLimpezaBackups;
    private bool _limpezaBackupsEmCurso;
    private string _estadoVerificacaoBackup = "Ainda não verificado";
    private string _detalheVerificacaoBackup = string.Empty;
    private bool _verificacaoBackupEmCurso;
    private string _mensagemPlanoRecuperacao = string.Empty;
    private bool _planoRecuperacaoEmCurso;
    private int _frequenciaBackupHoras = 24;
    private int _rpoObjetivoHoras = 24;
    private int _rtoObjetivoHoras = 8;
    private string _estadoPoliticaContinuidade = "Ainda não avaliada";
    private string _mensagemPoliticaContinuidade = string.Empty;
    private bool _politicaContinuidadeEmCurso;
    private DateTime? _ultimoBackupData;
    private string _estadoTesteContinuidade = "Ainda não executado";
    private string _resumoTesteContinuidade = string.Empty;
    private bool _testeContinuidadeEmCurso;

    public string EmpresaNome { get => _empresaNome; set => SetProperty(ref _empresaNome, value); }
    public string Moeda { get => _moeda; set => SetProperty(ref _moeda, value); }
    public string NIF { get => _nif; set => SetProperty(ref _nif, value); }
    public string Morada { get => _morada; set => SetProperty(ref _morada, value); }
    public string Telefone { get => _telefone; set => SetProperty(ref _telefone, value); }
    public string EmailEmpresa { get => _emailEmpresa; set => SetProperty(ref _emailEmpresa, value); }
    public string MensagemErroEmpresa { get => _mensagemErroEmpresa; set => SetProperty(ref _mensagemErroEmpresa, value); }
    public bool AGuardarEmpresa { get => _aGuardarEmpresa; set => SetProperty(ref _aGuardarEmpresa, value); }
    public ICommand GuardarEmpresaCommand { get; }
    public string Tema { get => _tema; set => SetProperty(ref _tema, value); }
    public bool PermitirCaixaNegativo { get => _permitirCaixaNegativo; set => SetProperty(ref _permitirCaixaNegativo, value); }
    public decimal LimiteAprovacaoPagamento { get => _limiteAprovacaoPagamento; set => SetProperty(ref _limiteAprovacaoPagamento, value); }
    public string MensagemParametros { get => _mensagemParametros; set => SetProperty(ref _mensagemParametros, value); }
    public ICommand GuardarParametrosCommand { get; }
    public string DatabaseNome { get => _databaseNome; set => SetProperty(ref _databaseNome, value); }
    public string DatabaseServidor { get => _databaseServidor; set => SetProperty(ref _databaseServidor, value); }
    public decimal DatabaseTamanhoMb { get => _databaseTamanhoMb; set => SetProperty(ref _databaseTamanhoMb, value); }
    public string UltimoBackup { get => _ultimoBackup; set => SetProperty(ref _ultimoBackup, value); }
    public string ResumoSistema { get => _resumoSistema; set => SetProperty(ref _resumoSistema, value); }
    public string MensagemBackup { get => _mensagemBackup; set => SetProperty(ref _mensagemBackup, value); }
    public bool BackupEmCurso { get => _backupEmCurso; set => SetProperty(ref _backupEmCurso, value); }
    public string SqlServerVersao { get => _sqlServerVersao; set => SetProperty(ref _sqlServerVersao, value); }
    public string DatabaseEstado { get => _databaseEstado; set => SetProperty(ref _databaseEstado, value); }
    public string RecoveryModel { get => _recoveryModel; set => SetProperty(ref _recoveryModel, value); }
    public int CompatibilityLevel { get => _compatibilityLevel; set => SetProperty(ref _compatibilityLevel, value); }
    public string IntegridadeBase { get => _integridadeBase; set => SetProperty(ref _integridadeBase, value); }
    public string MensagemDiagnostico { get => _mensagemDiagnostico; set => SetProperty(ref _mensagemDiagnostico, value); }
    public bool DiagnosticoEmCurso { get => _diagnosticoEmCurso; set => SetProperty(ref _diagnosticoEmCurso, value); }
    public string MensagemSuporte { get => _mensagemSuporte; set => SetProperty(ref _mensagemSuporte, value); }
    public bool RelatorioSuporteEmCurso { get => _relatorioSuporteEmCurso; set => SetProperty(ref _relatorioSuporteEmCurso, value); }
    public string EstadoProntidao { get => _estadoProntidao; set => SetProperty(ref _estadoProntidao, value); }
    public string ResumoProntidao { get => _resumoProntidao; set => SetProperty(ref _resumoProntidao, value); }
    public bool ProntidaoEmCurso { get => _prontidaoEmCurso; set => SetProperty(ref _prontidaoEmCurso, value); }
    public ObservableCollection<SystemReadinessItemDto> VerificacoesProntidao { get; } = new();
    public ObservableCollection<DatabaseBackupDto> HistoricoBackups { get; } = new();
    public string MensagemHistoricoBackups { get => _mensagemHistoricoBackups; set => SetProperty(ref _mensagemHistoricoBackups, value); }
    public bool HistoricoBackupsEmCurso { get => _historicoBackupsEmCurso; set => SetProperty(ref _historicoBackupsEmCurso, value); }
    public int BackupsAPreservar { get => _backupsAPreservar; set => SetProperty(ref _backupsAPreservar, value); }
    public bool ConfirmarLimpezaBackups { get => _confirmarLimpezaBackups; set => SetProperty(ref _confirmarLimpezaBackups, value); }
    public bool LimpezaBackupsEmCurso { get => _limpezaBackupsEmCurso; set => SetProperty(ref _limpezaBackupsEmCurso, value); }
    public string EstadoVerificacaoBackup { get => _estadoVerificacaoBackup; set => SetProperty(ref _estadoVerificacaoBackup, value); }
    public string DetalheVerificacaoBackup { get => _detalheVerificacaoBackup; set => SetProperty(ref _detalheVerificacaoBackup, value); }
    public bool VerificacaoBackupEmCurso { get => _verificacaoBackupEmCurso; set => SetProperty(ref _verificacaoBackupEmCurso, value); }
    public string MensagemPlanoRecuperacao { get => _mensagemPlanoRecuperacao; set => SetProperty(ref _mensagemPlanoRecuperacao, value); }
    public bool PlanoRecuperacaoEmCurso { get => _planoRecuperacaoEmCurso; set => SetProperty(ref _planoRecuperacaoEmCurso, value); }
    public int FrequenciaBackupHoras { get => _frequenciaBackupHoras; set => SetProperty(ref _frequenciaBackupHoras, value); }
    public int RpoObjetivoHoras { get => _rpoObjetivoHoras; set => SetProperty(ref _rpoObjetivoHoras, value); }
    public int RtoObjetivoHoras { get => _rtoObjetivoHoras; set => SetProperty(ref _rtoObjetivoHoras, value); }
    public string EstadoPoliticaContinuidade { get => _estadoPoliticaContinuidade; set => SetProperty(ref _estadoPoliticaContinuidade, value); }
    public string MensagemPoliticaContinuidade { get => _mensagemPoliticaContinuidade; set => SetProperty(ref _mensagemPoliticaContinuidade, value); }
    public bool PoliticaContinuidadeEmCurso { get => _politicaContinuidadeEmCurso; set => SetProperty(ref _politicaContinuidadeEmCurso, value); }
    public string EstadoTesteContinuidade { get => _estadoTesteContinuidade; set => SetProperty(ref _estadoTesteContinuidade, value); }
    public string ResumoTesteContinuidade { get => _resumoTesteContinuidade; set => SetProperty(ref _resumoTesteContinuidade, value); }
    public bool TesteContinuidadeEmCurso { get => _testeContinuidadeEmCurso; set => SetProperty(ref _testeContinuidadeEmCurso, value); }
    public ObservableCollection<ContinuityTestItemDto> TestesContinuidade { get; } = new();
    public ICommand AtualizarSaudeCommand { get; }
    public ICommand CriarBackupCommand { get; }
    public ICommand ExecutarDiagnosticoCommand { get; }
    public ICommand GerarRelatorioSuporteCommand { get; }
    public ICommand VerificarProntidaoCommand { get; }
    public ICommand AtualizarHistoricoBackupsCommand { get; }
    public ICommand LimparBackupsAntigosCommand { get; }
    public ICommand VerificarUltimoBackupCommand { get; }
    public ICommand GerarPlanoRecuperacaoCommand { get; }
    public ICommand GuardarPoliticaContinuidadeCommand { get; }
    public ICommand ExecutarTesteContinuidadeCommand { get; }

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

    public ConfiguracoesViewModel(IConfiguracoesService service, ISettingsService settings, int empresaId)
    {
        _service = service;
        _empresaId = empresaId;
        _settings = settings;

        GuardarEmpresaCommand = new AsyncRelayCommand(_ => GuardarEmpresaAsync(), _ => !AGuardarEmpresa);
        GuardarParametrosCommand = new AsyncRelayCommand(_ => GuardarParametrosAsync());
        AtualizarSaudeCommand = new AsyncRelayCommand(_ => CarregarSaudeAsync());
        CriarBackupCommand = new AsyncRelayCommand(_ => CriarBackupAsync(), _ => !BackupEmCurso);
        ExecutarDiagnosticoCommand = new AsyncRelayCommand(_ => ExecutarDiagnosticoAsync(), _ => !DiagnosticoEmCurso);
        GerarRelatorioSuporteCommand = new AsyncRelayCommand(_ => GerarRelatorioSuporteAsync(), _ => !RelatorioSuporteEmCurso);
        VerificarProntidaoCommand = new AsyncRelayCommand(_ => VerificarProntidaoAsync(), _ => !ProntidaoEmCurso);
        AtualizarHistoricoBackupsCommand = new AsyncRelayCommand(_ => CarregarHistoricoBackupsAsync(), _ => !HistoricoBackupsEmCurso);
        LimparBackupsAntigosCommand = new AsyncRelayCommand(_ => LimparBackupsAntigosAsync(), _ => !LimpezaBackupsEmCurso);
        VerificarUltimoBackupCommand = new AsyncRelayCommand(_ => VerificarUltimoBackupAsync(), _ => !VerificacaoBackupEmCurso);
        GerarPlanoRecuperacaoCommand = new AsyncRelayCommand(_ => GerarPlanoRecuperacaoAsync(), _ => !PlanoRecuperacaoEmCurso);
        GuardarPoliticaContinuidadeCommand = new AsyncRelayCommand(_ => GuardarPoliticaContinuidadeAsync(), _ => !PoliticaContinuidadeEmCurso);
        ExecutarTesteContinuidadeCommand = new AsyncRelayCommand(_ => ExecutarTesteContinuidadeAsync(), _ => !TesteContinuidadeEmCurso);
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
        Tema = await _settings.GetAsync(_empresaId, "UI", "Theme", "Light") ?? "Light";
        PermitirCaixaNegativo = await _settings.GetAsync(_empresaId, "Treasury", "AllowNegativeCash", false);
        LimiteAprovacaoPagamento = await _settings.GetAsync(_empresaId, "Workflow", "PaymentApprovalLimit", 0m);
        FrequenciaBackupHoras = await _settings.GetAsync(_empresaId, "Continuity", "BackupFrequencyHours", 24);
        BackupsAPreservar = await _settings.GetAsync(_empresaId, "Continuity", "BackupRetentionCount", 5);
        RpoObjetivoHoras = await _settings.GetAsync(_empresaId, "Continuity", "RpoHours", 24);
        RtoObjetivoHoras = await _settings.GetAsync(_empresaId, "Continuity", "RtoHours", 8);
        await CarregarSaudeAsync();
        await CarregarHistoricoBackupsAsync();
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

    private async Task GuardarParametrosAsync()
    {
        MensagemParametros = string.Empty;
        try
        {
            await _settings.SetAsync(_empresaId, "UI", "Theme", Tema, "Tema visual da aplicação");
            await _settings.SetAsync(_empresaId, "Treasury", "AllowNegativeCash", PermitirCaixaNegativo, "Permite saldo negativo de caixa");
            await _settings.SetAsync(_empresaId, "Workflow", "PaymentApprovalLimit", LimiteAprovacaoPagamento, "Limite para aprovação obrigatória de pagamentos");
            MensagemParametros = "Parâmetros guardados com sucesso.";
        }
        catch (Exception ex)
        {
            MensagemParametros = ex.Message;
        }
    }

    private async Task CarregarSaudeAsync()
    {
        try
        {
            var health = await _service.ObterSaudeSistemaAsync();
            DatabaseNome = health.DatabaseName;
            DatabaseServidor = health.DataSource;
            DatabaseTamanhoMb = health.DatabaseSizeMb;
            UltimoBackup = health.LastBackupAt?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca / indisponível";
            _ultimoBackupData = health.LastBackupAt;
            ResumoSistema = $"Empresas: {health.Empresas}  •  Utilizadores ativos: {health.UtilizadoresAtivos}  •  Eventos de auditoria: {health.LogsAuditoria}";
            AvaliarPoliticaContinuidade();
        }
        catch (Exception ex)
        {
            ResumoSistema = $"Não foi possível consultar a saúde do sistema: {ex.Message}";
        }
    }

    private async Task ExecutarDiagnosticoAsync()
    {
        MensagemDiagnostico = string.Empty;
        DiagnosticoEmCurso = true;
        try
        {
            var info = await _service.ExecutarDiagnosticoAsync();
            SqlServerVersao = info.SqlServerVersion;
            DatabaseEstado = info.DatabaseState;
            RecoveryModel = info.RecoveryModel;
            CompatibilityLevel = info.CompatibilityLevel;
            IntegridadeBase = info.IntegrityStatus;
            MensagemDiagnostico = $"Diagnóstico atualizado em {info.CheckedAt:dd/MM/yyyy HH:mm}.";
        }
        catch (Exception ex)
        {
            MensagemDiagnostico = $"Falha no diagnóstico: {ex.Message}";
        }
        finally
        {
            DiagnosticoEmCurso = false;
        }
    }

    private async Task CriarBackupAsync()
    {
        MensagemBackup = string.Empty;
        BackupEmCurso = true;
        try
        {
            var path = await _service.CriarBackupBaseDadosAsync();
            MensagemBackup = $"Backup concluído: {path}";
            await CarregarSaudeAsync();
        }
        catch (Exception ex)
        {
            MensagemBackup = $"Falha ao criar backup: {ex.Message}";
        }
        finally
        {
            BackupEmCurso = false;
        }
    }

    private async Task GerarRelatorioSuporteAsync()
    {
        MensagemSuporte = string.Empty;
        RelatorioSuporteEmCurso = true;
        try
        {
            var path = await _service.GerarRelatorioSuporteAsync();
            MensagemSuporte = $"Relatório técnico criado: {path}";
        }
        catch (Exception ex)
        {
            MensagemSuporte = $"Falha ao gerar relatório técnico: {ex.Message}";
        }
        finally
        {
            RelatorioSuporteEmCurso = false;
        }
    }

    private async Task VerificarProntidaoAsync()
    {
        ProntidaoEmCurso = true;
        ResumoProntidao = string.Empty;
        try
        {
            var result = await _service.VerificarProntidaoAsync();
            EstadoProntidao = result.OverallStatus;
            ResumoProntidao = $"OK: {result.Passed}  •  Avisos: {result.Warnings}  •  Falhas: {result.Failed}  •  {result.CheckedAt:dd/MM/yyyy HH:mm}";
            VerificacoesProntidao.Clear();
            foreach (var item in result.Items)
                VerificacoesProntidao.Add(item);
        }
        catch (Exception ex)
        {
            EstadoProntidao = "Falha na verificação";
            ResumoProntidao = ex.Message;
        }
        finally
        {
            ProntidaoEmCurso = false;
        }
    }

    private async Task CarregarHistoricoBackupsAsync()
    {
        HistoricoBackupsEmCurso = true;
        MensagemHistoricoBackups = string.Empty;
        try
        {
            var backups = await _service.ListarHistoricoBackupsAsync();
            HistoricoBackups.Clear();
            foreach (var backup in backups)
                HistoricoBackups.Add(backup);

            MensagemHistoricoBackups = backups.Count == 0
                ? "Não foram encontrados backups desta base de dados."
                : $"{backups.Count} backup(s) apresentado(s), do mais recente para o mais antigo.";
        }
        catch (Exception ex)
        {
            HistoricoBackups.Clear();
            MensagemHistoricoBackups = ex.Message;
        }
        finally
        {
            HistoricoBackupsEmCurso = false;
        }
    }

    private async Task LimparBackupsAntigosAsync()
    {
        if (!ConfirmarLimpezaBackups)
        {
            MensagemHistoricoBackups = "Marque a confirmação antes de eliminar backups antigos.";
            return;
        }

        if (BackupsAPreservar is < 1 or > 100)
        {
            MensagemHistoricoBackups = "Indique entre 1 e 100 backups a preservar.";
            return;
        }

        LimpezaBackupsEmCurso = true;
        MensagemHistoricoBackups = string.Empty;
        try
        {
            var result = await _service.LimparBackupsAntigosAsync(BackupsAPreservar);
            var resumoLimpeza = result.Deleted == 0
                ? $"Nenhum backup antigo eliminado. {result.Preserved} backup(s) preservado(s)."
                : $"Limpeza concluída: {result.Deleted} eliminado(s), {result.Preserved} preservado(s) e {result.ReleasedMb:N2} MB libertados.";
            ConfirmarLimpezaBackups = false;
            await CarregarSaudeAsync();
            await CarregarHistoricoBackupsAsync();
            MensagemHistoricoBackups = resumoLimpeza;
        }
        catch (Exception ex)
        {
            MensagemHistoricoBackups = $"Falha na limpeza: {ex.Message}";
        }
        finally
        {
            LimpezaBackupsEmCurso = false;
        }
    }

    private async Task VerificarUltimoBackupAsync()
    {
        VerificacaoBackupEmCurso = true;
        EstadoVerificacaoBackup = "A verificar...";
        DetalheVerificacaoBackup = string.Empty;
        try
        {
            var result = await _service.VerificarUltimoBackupAsync();
            EstadoVerificacaoBackup = result.IsValid ? "Backup íntegro" : "Backup inválido";
            DetalheVerificacaoBackup = $"{result.Status}  •  Backup: {result.BackupCompletedAt:dd/MM/yyyy HH:mm}  •  Verificado: {result.CheckedAt:dd/MM/yyyy HH:mm}";
        }
        catch (Exception ex)
        {
            EstadoVerificacaoBackup = "Verificação falhou";
            DetalheVerificacaoBackup = ex.Message;
        }
        finally
        {
            VerificacaoBackupEmCurso = false;
        }
    }

    private async Task GerarPlanoRecuperacaoAsync()
    {
        PlanoRecuperacaoEmCurso = true;
        MensagemPlanoRecuperacao = string.Empty;
        try
        {
            var path = await _service.GerarPlanoRecuperacaoAsync();
            MensagemPlanoRecuperacao = $"Plano de recuperação criado: {path}";
        }
        catch (Exception ex)
        {
            MensagemPlanoRecuperacao = $"Não foi possível gerar o plano: {ex.Message}";
        }
        finally
        {
            PlanoRecuperacaoEmCurso = false;
        }
    }

    private async Task GuardarPoliticaContinuidadeAsync()
    {
        if (FrequenciaBackupHoras is < 1 or > 720 ||
            BackupsAPreservar is < 1 or > 100 ||
            RpoObjetivoHoras is < 1 or > 720 ||
            RtoObjetivoHoras is < 1 or > 168)
        {
            MensagemPoliticaContinuidade = "Valores inválidos: frequência/RPO entre 1 e 720 horas, RTO entre 1 e 168 horas e retenção entre 1 e 100.";
            return;
        }

        PoliticaContinuidadeEmCurso = true;
        MensagemPoliticaContinuidade = string.Empty;
        try
        {
            await _settings.SetAsync(_empresaId, "Continuity", "BackupFrequencyHours", FrequenciaBackupHoras, "Frequência esperada dos backups, em horas");
            await _settings.SetAsync(_empresaId, "Continuity", "BackupRetentionCount", BackupsAPreservar, "Quantidade de backups recentes a preservar");
            await _settings.SetAsync(_empresaId, "Continuity", "RpoHours", RpoObjetivoHoras, "Objetivo de ponto de recuperação, em horas");
            await _settings.SetAsync(_empresaId, "Continuity", "RtoHours", RtoObjetivoHoras, "Objetivo de tempo de recuperação, em horas");
            AvaliarPoliticaContinuidade();
            MensagemPoliticaContinuidade = "Política de continuidade guardada e avaliada.";
        }
        catch (Exception ex)
        {
            MensagemPoliticaContinuidade = $"Não foi possível guardar a política: {ex.Message}";
        }
        finally
        {
            PoliticaContinuidadeEmCurso = false;
        }
    }

    private void AvaliarPoliticaContinuidade()
    {
        if (!_ultimoBackupData.HasValue)
        {
            EstadoPoliticaContinuidade = "Não conforme — sem backup";
            return;
        }

        var idadeHoras = Math.Max(0, (DateTime.Now - _ultimoBackupData.Value).TotalHours);
        EstadoPoliticaContinuidade = idadeHoras <= RpoObjetivoHoras
            ? $"Conforme — backup há {idadeHoras:N1} hora(s)"
            : $"Não conforme — RPO excedido em {idadeHoras - RpoObjetivoHoras:N1} hora(s)";
    }

    private async Task ExecutarTesteContinuidadeAsync()
    {
        TesteContinuidadeEmCurso = true;
        EstadoTesteContinuidade = "A executar...";
        ResumoTesteContinuidade = string.Empty;
        TestesContinuidade.Clear();
        try
        {
            var result = await _service.ExecutarTesteContinuidadeAsync(RpoObjetivoHoras, RtoObjetivoHoras);
            EstadoTesteContinuidade = result.OverallStatus;
            ResumoTesteContinuidade = $"Aprovados: {result.Passed}  •  Falhas: {result.Failed}  •  {result.CheckedAt:dd/MM/yyyy HH:mm}";
            foreach (var item in result.Items)
                TestesContinuidade.Add(item);
        }
        catch (Exception ex)
        {
            EstadoTesteContinuidade = "Teste não concluído";
            ResumoTesteContinuidade = ex.Message;
        }
        finally
        {
            TesteContinuidadeEmCurso = false;
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
