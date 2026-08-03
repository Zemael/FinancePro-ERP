using FinancePro.Application.Expenses;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.Expenses;

public sealed class ExpenseApplicationServiceTests
{
    [Fact]
    public async Task Criar_DeveFalhar_QuandoVencimentoAnteriorAEmissao()
    {
        var service = new ExpenseApplicationService(new FakeGateway());
        var result = await service.CriarAsync(new NovaContaPagarDto
        {
            EmpresaId = 1,
            Descricao = "Despesa",
            Valor = 100,
            DataEmissao = new DateTime(2026, 8, 2),
            DataVencimento = new DateTime(2026, 8, 1)
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RegistarPagamento_DeveFalhar_QuandoOrigemInvalida()
    {
        var service = new ExpenseApplicationService(new FakeGateway());
        var result = await service.RegistarPagamentoAsync(1, "Outro", 1, DateTime.Today);

        Assert.True(result.IsFailure);
    }

    private sealed class FakeGateway : IExpenseGateway
    {
        public Task<IReadOnlyList<ContaPagarListItemDto>> ListarAsync(int empresaId) => Task.FromResult<IReadOnlyList<ContaPagarListItemDto>>(Array.Empty<ContaPagarListItemDto>());
        public Task<IReadOnlyList<FornecedorOpcaoDto>> ListarFornecedoresAsync(int empresaId) => Task.FromResult<IReadOnlyList<FornecedorOpcaoDto>>(Array.Empty<FornecedorOpcaoDto>());
        public Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId) => Task.FromResult<IReadOnlyList<CategoriaOpcaoDto>>(Array.Empty<CategoriaOpcaoDto>());
        public Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId) => Task.FromResult<IReadOnlyList<OpcaoOrigemDto>>(Array.Empty<OpcaoOrigemDto>());
        public Task CriarAsync(NovaContaPagarDto dto) => Task.CompletedTask;
        public Task RegistarPagamentoAsync(int contaPagarId, string origemTipo, int origemId, DateTime dataPagamento) => Task.CompletedTask;
        public Task CancelarAsync(int contaPagarId) => Task.CompletedTask;
    }
}
