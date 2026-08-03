using FinancePro.Application.Payables;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.Payables;

public sealed class PayablesGateway : IPayablesGateway
{
    private readonly IDespesasService _service;

    public PayablesGateway(IDespesasService service) => _service = service;

    public Task<IReadOnlyList<ContaPagarListItemDto>> ListAsync(int empresaId, CancellationToken cancellationToken = default) => _service.ListarAsync(empresaId);
    public Task<IReadOnlyList<FornecedorOpcaoDto>> ListSuppliersAsync(int empresaId, CancellationToken cancellationToken = default) => _service.ListarFornecedoresAsync(empresaId);
    public Task<IReadOnlyList<CategoriaOpcaoDto>> ListCategoriesAsync(int empresaId, CancellationToken cancellationToken = default) => _service.ListarCategoriasAsync(empresaId);
    public Task<IReadOnlyList<OpcaoOrigemDto>> ListOriginsAsync(int empresaId, CancellationToken cancellationToken = default) => _service.ListarOrigensAsync(empresaId);
    public Task CreateAsync(NovaContaPagarDto request, CancellationToken cancellationToken = default) => _service.CriarAsync(request);
    public Task PayAsync(int contaPagarId, string origemTipo, int origemId, DateTime dataPagamento, CancellationToken cancellationToken = default) => _service.RegistarPagamentoAsync(contaPagarId, origemTipo, origemId, dataPagamento);
    public Task CancelAsync(int contaPagarId, CancellationToken cancellationToken = default) => _service.CancelarAsync(contaPagarId);
}
