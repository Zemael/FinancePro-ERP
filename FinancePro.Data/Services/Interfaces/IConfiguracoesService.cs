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
}
