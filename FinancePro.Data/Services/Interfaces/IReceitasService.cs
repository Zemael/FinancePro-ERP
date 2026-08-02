using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IReceitasService
{
    Task<IReadOnlyList<ContaReceberListItemDto>> ListarAsync(int empresaId);
    Task<IReadOnlyList<ClienteOpcaoDto>> ListarClientesAsync(int empresaId);
    Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId);
    Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId);
    Task CriarAsync(NovaContaReceberDto dto);
    Task RegistarRecebimentoAsync(int contaReceberId, string origemTipo, int origemId, DateTime dataRecebimento);
    Task CancelarAsync(int contaReceberId);
}
