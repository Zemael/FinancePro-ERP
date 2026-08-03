using FinancePro.Core.DTOs;

namespace FinancePro.Application.Payables;

public interface IPayablesGateway
{
    Task<IReadOnlyList<ContaPagarListItemDto>> ListAsync(int empresaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FornecedorOpcaoDto>> ListSuppliersAsync(int empresaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoriaOpcaoDto>> ListCategoriesAsync(int empresaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OpcaoOrigemDto>> ListOriginsAsync(int empresaId, CancellationToken cancellationToken = default);
    Task CreateAsync(NovaContaPagarDto request, CancellationToken cancellationToken = default);
    Task PayAsync(int contaPagarId, string origemTipo, int origemId, DateTime dataPagamento, CancellationToken cancellationToken = default);
    Task CancelAsync(int contaPagarId, CancellationToken cancellationToken = default);
}
