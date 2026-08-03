using FinancePro.Core.DTOs;

namespace FinancePro.Application.Suppliers;

public interface ISupplierGateway
{
    Task<IReadOnlyList<FornecedorDto>> ListAsync(int empresaId, string? pesquisa, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(SupplierSaveRequest request, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNifAsync(int empresaId, string nif, int ignoreId, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int id, bool ativo, CancellationToken cancellationToken = default);
}
