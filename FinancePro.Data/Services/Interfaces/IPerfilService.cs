using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IPerfilService
{
    Task<IReadOnlyList<PerfilDto>> ListarAsync(string? pesquisa = null);
    Task<int> GuardarAsync(PerfilDto dto);
    Task AlternarAtivoAsync(int id, bool ativo);
}
