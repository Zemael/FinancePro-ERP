using FinancePro.Application.Purchasing;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.Purchasing;

public sealed class PurchasingGateway : IPurchasingGateway
{
    private readonly ICompraService _service;

    public PurchasingGateway(ICompraService service)
    {
        _service = service;
    }

    public Task<IReadOnlyList<CompraListItemDto>> ListarAsync(int empresaId) =>
        _service.ListarAsync(empresaId);

    public Task<IReadOnlyList<FornecedorOpcaoDto>> ListarFornecedoresAsync(int empresaId) =>
        _service.ListarFornecedoresAsync(empresaId);

    public Task CriarAsync(NovaCompraDto dto) => _service.CriarAsync(dto);
    public Task CotarAsync(int compraId) => _service.CotarAsync(compraId);
    public Task AprovarAsync(int compraId) => _service.AprovarAsync(compraId);
    public Task RejeitarAsync(int compraId) => _service.RejeitarAsync(compraId);
    public Task CancelarAsync(int compraId) => _service.CancelarAsync(compraId);
    public Task EmitirOrdemAsync(int compraId) => _service.EmitirOrdemAsync(compraId);
    public Task ReceberAsync(int compraId) => _service.ReceberAsync(compraId);
    public Task FaturarAsync(int compraId) => _service.FaturarAsync(compraId);
}
