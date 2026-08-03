using FinancePro.Application.Budget;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using Xunit;

namespace FinancePro.Application.Tests.Budget;

public sealed class BudgetApplicationServiceTests
{
    [Fact]
    public async Task Criar_DeveFalhar_QuandoPeriodoInvalido()
    {
        var service = new BudgetApplicationService(new FakeGateway());
        var result = await service.CriarAsync(new NovoOrcamentoDto { EmpresaId = 1, Nome = "2026", Ano = 2026, DataInicio = new DateTime(2026,12,31), DataFim = new DateTime(2026,1,1) });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task AdicionarDetalhe_DeveFalhar_QuandoMesInvalido()
    {
        var service = new BudgetApplicationService(new FakeGateway());
        var result = await service.AdicionarDetalheAsync(new NovoOrcamentoDetalheDto { OrcamentoId = 1, PlanoContasId = 1, Mes = 13, ValorPrevisto = 10, Tipo = TipoCategoria.Despesa });
        Assert.True(result.IsFailure);
    }

    private sealed class FakeGateway : IBudgetGateway
    {
        public Task<IReadOnlyList<OrcamentoListItemDto>> ListarAsync(int empresaId) => Task.FromResult<IReadOnlyList<OrcamentoListItemDto>>([]);
        public Task<int> CriarAsync(NovoOrcamentoDto dto) => Task.FromResult(1);
        public Task<IReadOnlyList<PlanoContasOpcaoDto>> ListarPlanoContasAsync(int empresaId) => Task.FromResult<IReadOnlyList<PlanoContasOpcaoDto>>([]);
        public Task<IReadOnlyList<OrcamentoDetalheDto>> ListarDetalhesAsync(int id, TipoCategoria tipo) => Task.FromResult<IReadOnlyList<OrcamentoDetalheDto>>([]);
        public Task AdicionarDetalheAsync(NovoOrcamentoDetalheDto dto) => Task.CompletedTask;
        public Task<IReadOnlyList<ExecucaoMensalDto>> ObterExecucaoMensalAsync(int id) => Task.FromResult<IReadOnlyList<ExecucaoMensalDto>>([]);
        public Task<IReadOnlyList<RevisaoOrcamentalDto>> ListarRevisoesAsync(int id) => Task.FromResult<IReadOnlyList<RevisaoOrcamentalDto>>([]);
        public Task AdicionarRevisaoAsync(NovaRevisaoOrcamentalDto dto) => Task.CompletedTask;
        public Task<OrcamentoRelatorioDto> ObterRelatorioAsync(int id) => Task.FromResult(new OrcamentoRelatorioDto());
    }
}
