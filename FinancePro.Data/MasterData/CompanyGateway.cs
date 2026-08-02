using FinancePro.Application.MasterData.Companies;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.MasterData;

public sealed class CompanyGateway : ICompanyGateway
{
    private readonly IEmpresaService _service;

    public CompanyGateway(IEmpresaService service) => _service = service;

    public Task<IReadOnlyList<EmpresaListItemDto>> ListAsync(string? search, CancellationToken cancellationToken = default) =>
        _service.ListarAsync(search);

    public Task<EmpresaDto?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _service.ObterAsync(id);

    public Task<int> SaveAsync(CompanySaveRequest request, CancellationToken cancellationToken = default) =>
        _service.GuardarAsync(new EmpresaDto
        {
            Id = request.Id,
            Nome = request.Name,
            NIF = request.TaxNumber,
            Morada = request.Address,
            Telefone = request.Phone,
            Email = request.Email,
            Moeda = request.Currency
        });

    public Task SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default) =>
        _service.AlternarAtivoAsync(id, active);
}
