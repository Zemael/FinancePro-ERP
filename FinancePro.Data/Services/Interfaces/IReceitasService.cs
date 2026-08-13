using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IReceitasService
{
    Task<IReadOnlyList<ContaReceberListItemDto>> ListarAsync(int empresaId);
    Task<IReadOnlyList<ClienteOpcaoDto>> ListarClientesAsync(int empresaId);
    Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId);
    Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId);
    Task CriarAsync(NovaContaReceberDto dto);
    Task RegistarRecebimentoAsync(int contaReceberId, string origemTipo, int origemId, DateTime dataRecebimento);
    Task RegistarRecebimentoParcialAsync(int contaReceberId, decimal valor, string origemTipo, int origemId, DateTime dataRecebimento);
    Task AprovarPropostaAsync(int contaReceberId);
    Task FaturarAsync(int contaReceberId);
    Task CancelarAsync(int contaReceberId);
    Task<IReadOnlyList<ProdutoStockDto>> ListarProdutosAsync(int empresaId);
    Task<IReadOnlyList<DocumentoItemDto>> ListarItensAsync(int contaReceberId);
    Task AdicionarItemAsync(NovoDocumentoItemDto dto);
    Task RemoverItemAsync(int itemId);
    Task<IReadOnlyList<DocumentoFiscalDto>> ListarDocumentosAsync(int contaReceberId);
    Task EmitirNotaAsync(NovaNotaFiscalDto dto);
}
