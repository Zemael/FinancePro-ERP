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
    public Task<EmpresaDto?> ObterEmpresaAsync(int empresaId) => _service.ObterEmpresaAsync(empresaId);
    public Task<IReadOnlyList<ClienteOpcaoDto>> ListarClientesAsync(int empresaId) => _service.ListarClientesAsync(empresaId);
    public Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId) => _service.ListarCategoriasAsync(empresaId);
    public Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId) => _service.ListarOrigensAsync(empresaId);
    public Task CriarAsync(NovaContaReceberDto dto) => _service.CriarAsync(dto);
    public Task AtualizarRascunhoAsync(AtualizarFaturaRascunhoDto dto) => _service.AtualizarRascunhoAsync(dto);
    public Task<int> DuplicarAsync(int contaReceberId, int empresaId) => _service.DuplicarAsync(contaReceberId, empresaId);
    public Task RegistarRecebimentoAsync(int contaReceberId, string origemTipo, int origemId, DateTime dataRecebimento) =>
        _service.RegistarRecebimentoAsync(contaReceberId, origemTipo, origemId, dataRecebimento);
    public Task RegistarRecebimentoParcialAsync(int contaReceberId, decimal valor, string origemTipo, int origemId, DateTime dataRecebimento) =>
        _service.RegistarRecebimentoParcialAsync(contaReceberId, valor, origemTipo, origemId, dataRecebimento);
    public Task AprovarPropostaAsync(int contaReceberId) => _service.AprovarPropostaAsync(contaReceberId);
    public Task FaturarAsync(int contaReceberId) => _service.FaturarAsync(contaReceberId);
    public Task CancelarAsync(int contaReceberId) => _service.CancelarAsync(contaReceberId);

    public Task<IReadOnlyList<ProdutoStockDto>> ListarProdutosAsync(int empresaId) => _service.ListarProdutosAsync(empresaId);
    public Task<IReadOnlyList<DocumentoItemDto>> ListarItensAsync(int documentoId) => _service.ListarItensAsync(documentoId);
    public Task AdicionarItemAsync(NovoDocumentoItemDto dto) => _service.AdicionarItemAsync(dto);
    public Task AtualizarItemAsync(AtualizarDocumentoItemDto dto) => _service.AtualizarItemAsync(dto);
    public Task RemoverItemAsync(int itemId) => _service.RemoverItemAsync(itemId);
    public Task<IReadOnlyList<DocumentoFiscalDto>> ListarDocumentosAsync(int contaReceberId) => _service.ListarDocumentosAsync(contaReceberId);
    public Task EmitirNotaAsync(NovaNotaFiscalDto dto) => _service.EmitirNotaAsync(dto);
}
