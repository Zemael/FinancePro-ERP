using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IMoedaService
{
    Task<IReadOnlyList<MoedaDto>> ListarAsync(string? pesquisa = null);
    Task<int> GuardarAsync(MoedaDto dto);
    Task AlternarAtivoAsync(int id, bool ativo);
}
