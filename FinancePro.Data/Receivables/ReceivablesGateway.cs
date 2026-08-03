using FinancePro.Application.Receivables;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.Receivables;

public sealed class ReceivablesGateway : IReceivablesGateway
{
    private readonly IReceitasService _service;
    public ReceivablesGateway(IReceitasService service) => _service = service;

    public Task<IReadOnlyList<ContaReceberListItemDto>> ListAsync(int empresaId, CancellationToken cancellationToken = default) => _service.ListarAsync(empresaId);
    public Task<IReadOnlyList<ClienteOpcaoDto>> ListCustomersAsync(int empresaId, CancellationToken cancellationToken = default) => _service.ListarClientesAsync(empresaId);
    public Task<IReadOnlyList<CategoriaOpcaoDto>> ListCategoriesAsync(int empresaId, CancellationToken cancellationToken = default) => _service.ListarCategoriasAsync(empresaId);
    public Task<IReadOnlyList<OpcaoOrigemDto>> ListOriginsAsync(int empresaId, CancellationToken cancellationToken = default) => _service.ListarOrigensAsync(empresaId);
    public Task CreateAsync(NovaContaReceberDto request, CancellationToken cancellationToken = default) => _service.CriarAsync(request);
    public Task ReceiveAsync(int contaReceberId, string origemTipo, int origemId, DateTime dataRecebimento, CancellationToken cancellationToken = default) => _service.RegistarRecebimentoAsync(contaReceberId, origemTipo, origemId, dataRecebimento);
    public Task CancelAsync(int contaReceberId, CancellationToken cancellationToken = default) => _service.CancelarAsync(contaReceberId);
}
