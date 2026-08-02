using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IBancoService
{
    Task<IReadOnlyList<BancoListItemDto>> ListarBancosAsync();
    Task CriarBancoAsync(NovoBancoDto dto);

    Task<IReadOnlyList<ContaBancariaListItemDto>> ListarContasAsync(int empresaId);
    Task CriarContaAsync(NovaContaBancariaDto dto);
    Task AlternarAtivoContaAsync(int contaId, bool ativo);
}
