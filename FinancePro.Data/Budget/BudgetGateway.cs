using FinancePro.Application.Budget;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.Budget;

public sealed class BudgetGateway : IBudgetGateway
{
    private readonly IOrcamentoService _service;
    public BudgetGateway(IOrcamentoService service) => _service = service;
    public Task<IReadOnlyList<OrcamentoListItemDto>> ListarAsync(int empresaId) => _service.ListarAsync(empresaId);
    public Task<int> CriarAsync(NovoOrcamentoDto dto) => _service.CriarAsync(dto);
    public Task<IReadOnlyList<PlanoContasOpcaoDto>> ListarPlanoContasAsync(int empresaId) => _service.ListarPlanoContasAsync(empresaId);
    public Task<IReadOnlyList<OrcamentoDetalheDto>> ListarDetalhesAsync(int id, TipoCategoria tipo) => _service.ListarDetalhesAsync(id, tipo);
    public Task AdicionarDetalheAsync(NovoOrcamentoDetalheDto dto) => _service.AdicionarDetalheAsync(dto);
    public Task<IReadOnlyList<ExecucaoMensalDto>> ObterExecucaoMensalAsync(int id) => _service.ObterExecucaoMensalAsync(id);
    public Task<IReadOnlyList<RevisaoOrcamentalDto>> ListarRevisoesAsync(int id) => _service.ListarRevisoesAsync(id);
    public Task AdicionarRevisaoAsync(NovaRevisaoOrcamentalDto dto) => _service.AdicionarRevisaoAsync(dto);
    public Task<OrcamentoRelatorioDto> ObterRelatorioAsync(int id) => _service.ObterRelatorioAsync(id);
}
