using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IEmpresaService
{
    Task<IReadOnlyList<EmpresaListItemDto>> ListarAsync(string? pesquisa = null);
    Task<EmpresaDto?> ObterAsync(int id);
    Task<int> GuardarAsync(EmpresaDto dto);
    Task AlternarAtivoAsync(int id, bool ativo);
}
