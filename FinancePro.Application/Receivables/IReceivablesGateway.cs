using FinancePro.Core.DTOs;

namespace FinancePro.Application.Receivables;

public interface IReceivablesGateway
{
    Task<IReadOnlyList<ContaReceberListItemDto>> ListAsync(int empresaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClienteOpcaoDto>> ListCustomersAsync(int empresaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoriaOpcaoDto>> ListCategoriesAsync(int empresaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OpcaoOrigemDto>> ListOriginsAsync(int empresaId, CancellationToken cancellationToken = default);
    Task CreateAsync(NovaContaReceberDto request, CancellationToken cancellationToken = default);
    Task ReceiveAsync(int contaReceberId, string origemTipo, int origemId, DateTime dataRecebimento, CancellationToken cancellationToken = default);
    Task CancelAsync(int contaReceberId, CancellationToken cancellationToken = default);
}
