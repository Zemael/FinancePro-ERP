using FinancePro.Core.DTOs;

namespace FinancePro.Application.MasterData.Companies;

public interface ICompanyGateway
{
    Task<IReadOnlyList<EmpresaListItemDto>> ListAsync(string? search, CancellationToken cancellationToken = default);
    Task<EmpresaDto?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(CompanySaveRequest request, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default);
}
