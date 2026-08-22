using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IConfiguracoesService
{
    Task<bool> ExisteAlgumUtilizadorAsync(CancellationToken cancellationToken = default);
    Task<int> ConfigurarInicialAsync(ConfiguracaoInicialDto dto);

    Task<EmpresaDto?> ObterEmpresaAsync(int empresaId);
    Task AtualizarEmpresaAsync(EmpresaDto dto);

    Task<IReadOnlyList<PerfilOpcaoDto>> ListarPerfisAsync();
    Task<IReadOnlyList<UtilizadorListItemDto>> ListarUtilizadoresAsync(int empresaId);
    Task CriarUtilizadorAsync(NovoUtilizadorDto dto);
    Task AlternarAtivoUtilizadorAsync(int utilizadorId, bool ativo);

    Task<SystemHealthDto> ObterSaudeSistemaAsync();
    Task<SystemDiagnosticsDto> ExecutarDiagnosticoAsync();
    Task<string> CriarBackupBaseDadosAsync();
    Task<string> GerarRelatorioSuporteAsync();
    Task<SystemReadinessDto> VerificarProntidaoAsync();
    Task<IReadOnlyList<DatabaseBackupDto>> ListarHistoricoBackupsAsync(int limite = 20);
    Task<BackupCleanupResultDto> LimparBackupsAntigosAsync(int preservar = 5);
    Task<BackupVerificationDto> VerificarUltimoBackupAsync();
    Task<string> GerarPlanoRecuperacaoAsync();
    Task<ContinuityTestDto> ExecutarTesteContinuidadeAsync(int rpoObjetivoHoras, int rtoObjetivoHoras);
}
