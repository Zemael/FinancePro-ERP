using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;

namespace FinancePro.Application.Treasury;

public interface IAdvancedTreasuryGateway
{
    Task<IReadOnlyList<MovimentoListItemDto>> ListMovementsAsync(int empresaId, int maxRecords = 200, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OpcaoOrigemDto>> ListOriginsAsync(int empresaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoriaOpcaoDto>> ListCategoriesAsync(int empresaId, TipoCategoria tipo, CancellationToken cancellationToken = default);
    Task<int> RegisterMovementAsync(NovoMovimentoDto request, CancellationToken cancellationToken = default);
    Task RegisterTransferAsync(NovaTransferenciaDto request, CancellationToken cancellationToken = default);
    Task SetReconciledAsync(int movementId, bool reconciled, CancellationToken cancellationToken = default);
}
