using FinancePro.Application.Receivables;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.Receivables;

public class ReceivablesApplicationServiceTests
{
    [Fact]
    public async Task Create_rejects_due_date_before_issue_date()
    {
        var service = new ReceivablesApplicationService(new FakeGateway());
        var result = await service.CreateAsync(new NovaContaReceberDto
        {
            EmpresaId = 1,
            Descricao = "Fatura",
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
                new ContaReceberListItemDto { Id = 1, Codigo = "CR-1", Descricao = "Aberta", Valor = 100, EstadoExibicao = "Pendente", PodeReceber = true, DataVencimento = DateTime.Today },
                new ContaReceberListItemDto { Id = 2, Codigo = "CR-2", Descricao = "Atrasada", Valor = 200, EstadoExibicao = "Atrasado", PodeReceber = true, DataVencimento = DateTime.Today.AddDays(-2) }
            }
        };
        var service = new ReceivablesApplicationService(gateway);
        var result = await service.LoadAsync(new ReceivablesQuery(1, Status: "Atrasado"));
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(300, result.Value.Summary.OpenAmount);
        Assert.Equal(200, result.Value.Summary.OverdueAmount);
    }

    private sealed class FakeGateway : IReceivablesGateway
    {
        public IReadOnlyList<ContaReceberListItemDto> Items { get; set; } = Array.Empty<ContaReceberListItemDto>();
        public Task CancelAsync(int contaReceberId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CreateAsync(NovaContaReceberDto request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<CategoriaOpcaoDto>> ListCategoriesAsync(int empresaId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<CategoriaOpcaoDto>>(Array.Empty<CategoriaOpcaoDto>());
        public Task<IReadOnlyList<ClienteOpcaoDto>> ListCustomersAsync(int empresaId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ClienteOpcaoDto>>(Array.Empty<ClienteOpcaoDto>());
        public Task<IReadOnlyList<OpcaoOrigemDto>> ListOriginsAsync(int empresaId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OpcaoOrigemDto>>(Array.Empty<OpcaoOrigemDto>());
        public Task<IReadOnlyList<ContaReceberListItemDto>> ListAsync(int empresaId, CancellationToken cancellationToken = default) => Task.FromResult(Items);
        public Task ReceiveAsync(int contaReceberId, string origemTipo, int origemId, DateTime dataRecebimento, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
