using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface ICompraService
{
    Task<IReadOnlyList<CompraListItemDto>> ListarAsync(int empresaId);
    Task<IReadOnlyList<FornecedorOpcaoDto>> ListarFornecedoresAsync(int empresaId);
    Task CriarAsync(NovaCompraDto dto);
    Task CotarAsync(int compraId);
    Task AprovarAsync(int compraId);
    Task RejeitarAsync(int compraId);
    Task CancelarAsync(int compraId);
    Task EmitirOrdemAsync(int compraId);
    Task ReceberAsync(int compraId);
    Task FaturarAsync(int compraId);
}
