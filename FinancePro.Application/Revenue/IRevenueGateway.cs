using FinancePro.Core.DTOs;

namespace FinancePro.Application.Revenue;

public interface IRevenueGateway
{
    Task<IReadOnlyList<ContaReceberListItemDto>> ListarAsync(int empresaId);
    Task<EmpresaDto?> ObterEmpresaAsync(int empresaId);
    Task<IReadOnlyList<ClienteOpcaoDto>> ListarClientesAsync(int empresaId);
    Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId);
    Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId);
    Task CriarAsync(NovaContaReceberDto dto);
    Task AtualizarRascunhoAsync(AtualizarFaturaRascunhoDto dto);
    Task<int> DuplicarAsync(int contaReceberId, int empresaId);
    Task RegistarRecebimentoAsync(int contaReceberId, string origemTipo, int origemId, DateTime dataRecebimento);
    Task RegistarRecebimentoParcialAsync(int contaReceberId, decimal valor, string origemTipo, int origemId, DateTime dataRecebimento);
    Task AprovarPropostaAsync(int contaReceberId);
    Task FaturarAsync(int contaReceberId);
    Task CancelarAsync(int contaReceberId);
    Task<IReadOnlyList<ProdutoStockDto>> ListarProdutosAsync(int empresaId);
    Task<IReadOnlyList<DocumentoItemDto>> ListarItensAsync(int documentoId);
    Task AdicionarItemAsync(NovoDocumentoItemDto dto);
    Task AtualizarItemAsync(AtualizarDocumentoItemDto dto);
    Task RemoverItemAsync(int itemId);
    Task<IReadOnlyList<DocumentoFiscalDto>> ListarDocumentosAsync(int contaReceberId);
    Task EmitirNotaAsync(NovaNotaFiscalDto dto);
}
