using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IUtilizadorService
{
    Task<IReadOnlyList<UtilizadorDto>> ListarAsync(string? pesquisa = null);
    Task<int> GuardarAsync(UtilizadorDto dto);
    Task AlternarAtivoAsync(int id, bool ativo);
}
