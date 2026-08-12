using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IDespesasService
{
    Task<IReadOnlyList<ContaPagarListItemDto>> ListarAsync(int empresaId);
    Task<IReadOnlyList<FornecedorOpcaoDto>> ListarFornecedoresAsync(int empresaId);
    Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId);
    Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId);
    Task CriarAsync(NovaContaPagarDto dto);
    Task RegistarPagamentoAsync(int contaPagarId, string origemTipo, int origemId, DateTime dataPagamento);
    Task RegistarPagamentoParcialAsync(int contaPagarId, decimal valor, string origemTipo, int origemId, DateTime dataPagamento);
    Task CancelarAsync(int contaPagarId);
}
