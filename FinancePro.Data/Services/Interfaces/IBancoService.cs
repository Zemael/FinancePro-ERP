using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IBancoService
{
    Task<IReadOnlyList<BancoListItemDto>> ListarBancosAsync();
    Task CriarBancoAsync(NovoBancoDto dto);
    Task AtualizarBancoAsync(int bancoId, NovoBancoDto dto);

    Task<IReadOnlyList<ContaBancariaListItemDto>> ListarContasAsync(int empresaId);
    Task CriarContaAsync(NovaContaBancariaDto dto);
    Task AtualizarContaAsync(int contaId, NovaContaBancariaDto dto);
    Task AlternarAtivoContaAsync(int contaId, bool ativo);
}
