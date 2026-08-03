using FinancePro.Application.Treasury;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using Xunit;

namespace FinancePro.Application.Tests.Treasury;

public sealed class AdvancedTreasuryApplicationServiceTests
{
    [Fact]
    public async Task RegisterTransfer_rejects_same_origin_and_destination()
    {
        var service = new AdvancedTreasuryApplicationService(new FakeGateway());
        var result = await service.RegisterTransferAsync(new NovaTransferenciaDto
        {
            EmpresaId = 1, Data = DateTime.Today, Descricao = "Teste", Valor = 100,
            OrigemTipo = "Caixa", OrigemId = 1, DestinoTipo = "Caixa", DestinoId = 1
        });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RegisterMovement_requires_exactly_one_origin()
    {
        var service = new AdvancedTreasuryApplicationService(new FakeGateway());
        var result = await service.RegisterMovementAsync(new NovoMovimentoDto
        {
            EmpresaId = 1, Data = DateTime.Today, Descricao = "Entrada", Valor = 100,
            TipoOperacao = TipoOperacao.Entrada
        });
        Assert.True(result.IsFailure);
    }

    private sealed class FakeGateway : IAdvancedTreasuryGateway
    {
        public Task<IReadOnlyList<MovimentoListItemDto>> ListMovementsAsync(int empresaId, int maxRecords = 200, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<MovimentoListItemDto>>(Array.Empty<MovimentoListItemDto>());
        public Task<IReadOnlyList<OpcaoOrigemDto>> ListOriginsAsync(int empresaId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OpcaoOrigemDto>>(Array.Empty<OpcaoOrigemDto>());
        public Task<IReadOnlyList<CategoriaOpcaoDto>> ListCategoriesAsync(int empresaId, TipoCategoria tipo, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<CategoriaOpcaoDto>>(Array.Empty<CategoriaOpcaoDto>());
        public Task<int> RegisterMovementAsync(NovoMovimentoDto request, CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task RegisterTransferAsync(NovaTransferenciaDto request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetReconciledAsync(int movementId, bool reconciled, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
