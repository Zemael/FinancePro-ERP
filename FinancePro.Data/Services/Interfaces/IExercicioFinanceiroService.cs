using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IExercicioFinanceiroService
{
    Task<IReadOnlyList<ExercicioFinanceiroDto>> ListarAsync(string? pesquisa = null);
    Task<int> GuardarAsync(ExercicioFinanceiroDto dto);
    Task AlternarAtivoAsync(int id, bool ativo);
}
