using FinancePro.Application.Expenses;
using FinancePro.Core.DTOs;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.Expenses;

public sealed class ExpenseGateway : IExpenseGateway
{
    private readonly IDespesasService _service;

    public ExpenseGateway(IDespesasService service)
    {
        _service = service;
    }

    public Task<IReadOnlyList<ContaPagarListItemDto>> ListarAsync(int empresaId) => _service.ListarAsync(empresaId);
    public Task<IReadOnlyList<FornecedorOpcaoDto>> ListarFornecedoresAsync(int empresaId) => _service.ListarFornecedoresAsync(empresaId);
    public Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId) => _service.ListarCategoriasAsync(empresaId);
    public Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId) => _service.ListarOrigensAsync(empresaId);
    public Task CriarAsync(NovaContaPagarDto dto) => _service.CriarAsync(dto);
    public Task RegistarPagamentoAsync(int contaPagarId, string origemTipo, int origemId, DateTime dataPagamento) =>
        _service.RegistarPagamentoAsync(contaPagarId, origemTipo, origemId, dataPagamento);
    public Task RegistarPagamentoParcialAsync(int contaPagarId, decimal valor, string origemTipo, int origemId, DateTime dataPagamento) =>
        _service.RegistarPagamentoParcialAsync(contaPagarId, valor, origemTipo, origemId, dataPagamento);
    public Task CancelarAsync(int contaPagarId) => _service.CancelarAsync(contaPagarId);
}
