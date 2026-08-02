using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;

namespace FinancePro.Application.Budget;

public interface IBudgetGateway
{
    Task<IReadOnlyList<OrcamentoListItemDto>> ListarAsync(int empresaId);
    Task<int> CriarAsync(NovoOrcamentoDto dto);
    Task<IReadOnlyList<PlanoContasOpcaoDto>> ListarPlanoContasAsync(int empresaId);
    Task<IReadOnlyList<OrcamentoDetalheDto>> ListarDetalhesAsync(int orcamentoId, TipoCategoria tipo);
    Task AdicionarDetalheAsync(NovoOrcamentoDetalheDto dto);
    Task<IReadOnlyList<ExecucaoMensalDto>> ObterExecucaoMensalAsync(int orcamentoId);
    Task<IReadOnlyList<RevisaoOrcamentalDto>> ListarRevisoesAsync(int orcamentoId);
    Task AdicionarRevisaoAsync(NovaRevisaoOrcamentalDto dto);
    Task<OrcamentoRelatorioDto> ObterRelatorioAsync(int orcamentoId);
}
