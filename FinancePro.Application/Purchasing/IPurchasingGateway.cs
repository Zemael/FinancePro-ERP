using FinancePro.Core.DTOs;

namespace FinancePro.Application.Purchasing;

public interface IPurchasingGateway
{
    Task<IReadOnlyList<CompraListItemDto>> ListarAsync(int empresaId);
    Task<IReadOnlyList<FornecedorOpcaoDto>> ListarFornecedoresAsync(int empresaId);
    Task CriarAsync(NovaCompraDto dto);
    Task AprovarAsync(int compraId);
    Task RejeitarAsync(int compraId);
    Task CancelarAsync(int compraId);
}
