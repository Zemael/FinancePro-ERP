using FinancePro.Application.Purchasing;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.Purchasing;

public sealed class PurchasingApplicationServiceTests
{
    [Fact]
    public async Task Criar_DeveFalhar_QuandoFornecedorNaoFoiSelecionado()
    {
        var service = new PurchasingApplicationService(new FakeGateway());

        var result = await service.CriarAsync(new NovaCompraDto
        {
            EmpresaId = 1,
            Data = DateTime.Today,
            Departamento = "Administração",
            Comprador = "Zemael",
            ValorTotal = 1000m
        });

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, erro => erro.Contains("fornecedor", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Criar_DeveTerSucesso_QuandoDadosSaoValidos()
    {
        var gateway = new FakeGateway();
        var service = new PurchasingApplicationService(gateway);

        var result = await service.CriarAsync(new NovaCompraDto
        {
            EmpresaId = 1,
            Data = DateTime.Today,
            Departamento = "Administração",
            Comprador = "Zemael",
            FornecedorId = 1,
            ValorTotal = 1000m
        });

        Assert.True(result.IsSuccess);
        Assert.True(gateway.CriarFoiChamado);
    }

    [Fact]
    public async Task Aprovar_DeveFalhar_QuandoIdInvalido()
    {
        var service = new PurchasingApplicationService(new FakeGateway());
        var result = await service.AprovarAsync(0);
        Assert.True(result.IsFailure);
    }

    private sealed class FakeGateway : IPurchasingGateway
    {
        public bool CriarFoiChamado { get; private set; }

        public Task<IReadOnlyList<CompraListItemDto>> ListarAsync(int empresaId) =>
            Task.FromResult<IReadOnlyList<CompraListItemDto>>([]);

        public Task<IReadOnlyList<FornecedorOpcaoDto>> ListarFornecedoresAsync(int empresaId) =>
            Task.FromResult<IReadOnlyList<FornecedorOpcaoDto>>([]);

        public Task CriarAsync(NovaCompraDto dto)
        {
            CriarFoiChamado = true;
            return Task.CompletedTask;
        }

        public Task AprovarAsync(int compraId) => Task.CompletedTask;
        public Task RejeitarAsync(int compraId) => Task.CompletedTask;
        public Task CancelarAsync(int compraId) => Task.CompletedTask;
    }
}
