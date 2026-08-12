using FinancePro.Application.Treasury;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using Xunit;

namespace FinancePro.Application.Tests.Treasury;

public sealed class TreasuryApplicationServiceTests
{
    [Fact]
    public async Task RegistarMovimento_DeveFalhar_QuandoDescricaoVazia()
    {
        var service = new TreasuryApplicationService(new FakeGateway());
        var result = await service.RegistarMovimentoAsync(new NovoMovimentoDto
        {
            EmpresaId = 1,
            Valor = 100,
            CaixaId = 1,
            TipoOperacao = TipoOperacao.Entrada
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RegistarTransferencia_DeveFalhar_QuandoOrigemEDestinoIguais()
    {
        var service = new TreasuryApplicationService(new FakeGateway());
        var result = await service.RegistarTransferenciaAsync(new NovaTransferenciaDto
        {
            EmpresaId = 1,
            Descricao = "Transferência",
            Valor = 100,
            OrigemTipo = "Caixa",
            OrigemId = 1,
            DestinoTipo = "Caixa",
            DestinoId = 1
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ObterResumo_DeveFalhar_QuandoEmpresaInvalida()
    {
        var service = new TreasuryApplicationService(new FakeGateway());
        var result = await service.ObterResumoAsync(0);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ObterAging_DeveFalhar_QuandoEmpresaInvalida()
    {
        var service = new TreasuryApplicationService(new FakeGateway());
        var result = await service.ObterAgingAsync(0);
        Assert.True(result.IsFailure);
    }

    private sealed class FakeGateway : ITreasuryGateway
    {
        public Task<IReadOnlyList<MovimentoListItemDto>> ListarMovimentosAsync(int empresaId, int maxRegistos = 100) => Task.FromResult<IReadOnlyList<MovimentoListItemDto>>(Array.Empty<MovimentoListItemDto>());
        public Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId) => Task.FromResult<IReadOnlyList<OpcaoOrigemDto>>(Array.Empty<OpcaoOrigemDto>());
        public Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId, TipoCategoria tipo) => Task.FromResult<IReadOnlyList<CategoriaOpcaoDto>>(Array.Empty<CategoriaOpcaoDto>());
        public Task<int> RegistarMovimentoAsync(NovoMovimentoDto dto) => Task.FromResult(1);
        public Task RegistarTransferenciaAsync(NovaTransferenciaDto dto) => Task.CompletedTask;
        public Task MarcarConciliadoAsync(int movimentoId, bool conciliado) => Task.CompletedTask;
        public Task<TreasuryOverviewDto> ObterResumoAsync(int empresaId) => Task.FromResult(new TreasuryOverviewDto());
        public Task<IReadOnlyList<TreasuryForecastItemDto>> ListarPrevisaoAsync(int empresaId, int dias = 30) => Task.FromResult<IReadOnlyList<TreasuryForecastItemDto>>(Array.Empty<TreasuryForecastItemDto>());
        public Task<IReadOnlyList<TreasuryAgingDto>> ObterAgingAsync(int empresaId) => Task.FromResult<IReadOnlyList<TreasuryAgingDto>>(Array.Empty<TreasuryAgingDto>());
    }
}
