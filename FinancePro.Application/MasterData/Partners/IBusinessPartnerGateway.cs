using FinancePro.Core.DTOs;

namespace FinancePro.Application.MasterData.Partners;

public interface IBusinessPartnerGateway
{
    Task<IReadOnlyList<BusinessPartnerDto>> ListAsync(int empresaId, string? tipo, string? pesquisa, CancellationToken cancellationToken = default);
    Task<BusinessPartnerDto?> GetAsync(int empresaId, string tipo, int id, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(BusinessPartnerSaveRequest request, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int empresaId, string tipo, int id, bool ativo, CancellationToken cancellationToken = default);
}
