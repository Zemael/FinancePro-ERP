using FinancePro.Application.Revenue;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.Revenue;

public sealed class RevenueApplicationServiceTests
{
    [Fact]
    public async Task Criar_DeveFalhar_QuandoVencimentoAnteriorAEmissao()
    {
        var service = new RevenueApplicationService(new FakeGateway());
        var result = await service.CriarAsync(new NovaContaReceberDto
        {
            EmpresaId = 1,
            Descricao = "Receita",
            Valor = 100,
            DataEmissao = new DateTime(2026, 8, 2),
            DataVencimento = new DateTime(2026, 8, 1)
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RegistarRecebimento_DeveFalhar_QuandoOrigemInvalida()
    {
        var service = new RevenueApplicationService(new FakeGateway());
        var result = await service.RegistarRecebimentoAsync(1, "Outro", 1, DateTime.Today);

        Assert.True(result.IsFailure);
    }


    [Fact]
    public async Task AprovarProposta_DeveFalhar_QuandoIdInvalido()
    {
        var service = new RevenueApplicationService(new FakeGateway());
        var result = await service.AprovarPropostaAsync(0);
        Assert.True(result.IsFailure);
    }

    private sealed class FakeGateway : IRevenueGateway
    {
        public Task<IReadOnlyList<ContaReceberListItemDto>> ListarAsync(int empresaId) => Task.FromResult<IReadOnlyList<ContaReceberListItemDto>>(Array.Empty<ContaReceberListItemDto>());
        public Task<EmpresaDto?> ObterEmpresaAsync(int empresaId) => Task.FromResult<EmpresaDto?>(new EmpresaDto { Id = empresaId, Nome = "Empresa" });
        public Task<IReadOnlyList<ClienteOpcaoDto>> ListarClientesAsync(int empresaId) => Task.FromResult<IReadOnlyList<ClienteOpcaoDto>>(Array.Empty<ClienteOpcaoDto>());
        public Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId) => Task.FromResult<IReadOnlyList<CategoriaOpcaoDto>>(Array.Empty<CategoriaOpcaoDto>());
        public Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId) => Task.FromResult<IReadOnlyList<OpcaoOrigemDto>>(Array.Empty<OpcaoOrigemDto>());
        public Task CriarAsync(NovaContaReceberDto dto) => Task.CompletedTask;
        public Task AtualizarRascunhoAsync(AtualizarFaturaRascunhoDto dto) => Task.CompletedTask;
        public Task<int> DuplicarAsync(int contaReceberId, int empresaId) => Task.FromResult(2);
        public Task RegistarRecebimentoAsync(int contaReceberId, string origemTipo, int origemId, DateTime dataRecebimento) => Task.CompletedTask;
        public Task RegistarRecebimentoParcialAsync(int contaReceberId, decimal valor, string origemTipo, int origemId, DateTime dataRecebimento) => Task.CompletedTask;
        public Task AprovarPropostaAsync(int contaReceberId) => Task.CompletedTask;
        public Task FaturarAsync(int contaReceberId) => Task.CompletedTask;
        public Task CancelarAsync(int contaReceberId) => Task.CompletedTask;

        public Task<IReadOnlyList<ProdutoStockDto>> ListarProdutosAsync(int empresaId) =>
            Task.FromResult<IReadOnlyList<ProdutoStockDto>>(Array.Empty<ProdutoStockDto>());

        public Task<IReadOnlyList<DocumentoItemDto>> ListarItensAsync(int documentoId) =>
            Task.FromResult<IReadOnlyList<DocumentoItemDto>>(Array.Empty<DocumentoItemDto>());

        public Task AdicionarItemAsync(NovoDocumentoItemDto dto) => Task.CompletedTask;
        public Task AtualizarItemAsync(AtualizarDocumentoItemDto dto) => Task.CompletedTask;

        public Task RemoverItemAsync(int itemId) => Task.CompletedTask;
        public Task<IReadOnlyList<DocumentoFiscalDto>> ListarDocumentosAsync(int contaReceberId) => Task.FromResult<IReadOnlyList<DocumentoFiscalDto>>(Array.Empty<DocumentoFiscalDto>());
        public Task EmitirNotaAsync(NovaNotaFiscalDto dto) => Task.CompletedTask;
    }
}
