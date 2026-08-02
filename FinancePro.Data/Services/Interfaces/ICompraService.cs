using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface ICompraService
{
    Task<IReadOnlyList<CompraListItemDto>> ListarAsync(int empresaId);
    Task<IReadOnlyList<FornecedorOpcaoDto>> ListarFornecedoresAsync(int empresaId);
    Task CriarAsync(NovaCompraDto dto);
    Task AprovarAsync(int compraId);
    Task RejeitarAsync(int compraId);
    Task CancelarAsync(int compraId);
}
