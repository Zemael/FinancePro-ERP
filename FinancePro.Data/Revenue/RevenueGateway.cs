using FinancePro.Application.Revenue;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.Revenue;

public sealed class RevenueGateway : IRevenueGateway
{
    private readonly IReceitasService _service;

    public RevenueGateway(IReceitasService service)
    {
        _service = service;
    }

    public Task<IReadOnlyList<ContaReceberListItemDto>> ListarAsync(int empresaId) => _service.ListarAsync(empresaId);
    public Task<IReadOnlyList<ClienteOpcaoDto>> ListarClientesAsync(int empresaId) => _service.ListarClientesAsync(empresaId);
    public Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId) => _service.ListarCategoriasAsync(empresaId);
    public Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId) => _service.ListarOrigensAsync(empresaId);
    public Task CriarAsync(NovaContaReceberDto dto) => _service.CriarAsync(dto);
    public Task RegistarRecebimentoAsync(int contaReceberId, string origemTipo, int origemId, DateTime dataRecebimento) =>
        _service.RegistarRecebimentoAsync(contaReceberId, origemTipo, origemId, dataRecebimento);
    public Task RegistarRecebimentoParcialAsync(int contaReceberId, decimal valor, string origemTipo, int origemId, DateTime dataRecebimento) =>
        _service.RegistarRecebimentoParcialAsync(contaReceberId, valor, origemTipo, origemId, dataRecebimento);
    public Task CancelarAsync(int contaReceberId) => _service.CancelarAsync(contaReceberId);
}
