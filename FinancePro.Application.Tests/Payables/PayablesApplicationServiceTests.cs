using FinancePro.Application.Payables;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.Payables;

public class PayablesApplicationServiceTests
{
    [Fact]
    public async Task Create_rejects_due_date_before_issue_date()
    {
        var service = new PayablesApplicationService(new FakeGateway());
        var result = await service.CreateAsync(new NovaContaPagarDto
        {
            EmpresaId = 1,
            Descricao = "Fatura fornecedor",
            Valor = 100,
            DataEmissao = new DateTime(2026, 8, 10),
            DataVencimento = new DateTime(2026, 8, 9)
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Load_filters_by_status_and_builds_summary()
    {
        var gateway = new FakeGateway
        {
            Items = new[]
            {
                new ContaPagarListItemDto { Id = 1, Codigo = "CP-1", Descricao = "Aberta", Valor = 100, EstadoExibicao = "Pendente", PodePagar = true, DataVencimento = DateTime.Today },
                new ContaPagarListItemDto { Id = 2, Codigo = "CP-2", Descricao = "Atrasada", Valor = 200, EstadoExibicao = "Atrasado", PodePagar = true, DataVencimento = DateTime.Today.AddDays(-2) }
            }
        };

        var service = new PayablesApplicationService(gateway);
        var result = await service.LoadAsync(new PayablesQuery(1, Status: "Atrasado"));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(300, result.Value.Summary.OpenAmount);
        Assert.Equal(200, result.Value.Summary.OverdueAmount);
    }

    [Fact]
    public async Task Pay_requires_available_origin()
    {
        var service = new PayablesApplicationService(new FakeGateway());
        var result = await service.PayAsync(1, new OpcaoOrigemDto { Id = 1, Tipo = "Caixa", Nome = "Caixa", Disponivel = false }, DateTime.Today);
        Assert.True(result.IsFailure);
    }

    private sealed class FakeGateway : IPayablesGateway
    {
        public IReadOnlyList<ContaPagarListItemDto> Items { get; set; } = Array.Empty<ContaPagarListItemDto>();
        public Task CancelAsync(int contaPagarId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CreateAsync(NovaContaPagarDto request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<CategoriaOpcaoDto>> ListCategoriesAsync(int empresaId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<CategoriaOpcaoDto>>(Array.Empty<CategoriaOpcaoDto>());
        public Task<IReadOnlyList<OpcaoOrigemDto>> ListOriginsAsync(int empresaId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OpcaoOrigemDto>>(Array.Empty<OpcaoOrigemDto>());
        public Task<IReadOnlyList<FornecedorOpcaoDto>> ListSuppliersAsync(int empresaId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<FornecedorOpcaoDto>>(Array.Empty<FornecedorOpcaoDto>());
        public Task<IReadOnlyList<ContaPagarListItemDto>> ListAsync(int empresaId, CancellationToken cancellationToken = default) => Task.FromResult(Items);
        public Task PayAsync(int contaPagarId, string origemTipo, int origemId, DateTime dataPagamento, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
