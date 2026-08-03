using FinancePro.Application.Treasury;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.Treasury;

public sealed class AdvancedTreasuryGateway : IAdvancedTreasuryGateway
{
    private readonly ITesourariaService _service;
    public AdvancedTreasuryGateway(ITesourariaService service) => _service = service;

    public Task<IReadOnlyList<MovimentoListItemDto>> ListMovementsAsync(int empresaId, int maxRecords = 200, CancellationToken cancellationToken = default) => _service.ListarMovimentosAsync(empresaId, maxRecords);
    public Task<IReadOnlyList<OpcaoOrigemDto>> ListOriginsAsync(int empresaId, CancellationToken cancellationToken = default) => _service.ListarOrigensAsync(empresaId);
    public Task<IReadOnlyList<CategoriaOpcaoDto>> ListCategoriesAsync(int empresaId, TipoCategoria tipo, CancellationToken cancellationToken = default) => _service.ListarCategoriasAsync(empresaId, tipo);
    public Task<int> RegisterMovementAsync(NovoMovimentoDto request, CancellationToken cancellationToken = default) => _service.RegistarMovimentoAsync(request);
    public Task RegisterTransferAsync(NovaTransferenciaDto request, CancellationToken cancellationToken = default) => _service.RegistarTransferenciaAsync(request);
    public Task SetReconciledAsync(int movementId, bool reconciled, CancellationToken cancellationToken = default) => _service.MarcarConciliadoAsync(movementId, reconciled);
}
