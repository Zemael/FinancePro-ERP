using FinancePro.Core.DTOs;

namespace FinancePro.Application.Customers;

public interface ICustomerGateway
{
    Task<IReadOnlyList<ClienteDto>> ListAsync(int empresaId, string? pesquisa, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(CustomerSaveRequest request, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNifAsync(int empresaId, string nif, int ignoreId, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int id, bool ativo, CancellationToken cancellationToken = default);
}
