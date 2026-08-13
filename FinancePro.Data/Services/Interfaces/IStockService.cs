using FinancePro.Core.DTOs;
namespace FinancePro.Services.Interfaces;
public interface IStockService { Task<IReadOnlyList<ProdutoStockDto>> ListarProdutosAsync(int empresaId); Task<IReadOnlyList<MovimentoStockDto>> ListarMovimentosAsync(int empresaId); Task CriarProdutoAsync(NovoProdutoDto dto); Task MovimentarAsync(NovoMovimentoStockDto dto); }
