using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IExercicioFinanceiroService
{
    Task<IReadOnlyList<ExercicioFinanceiroDto>> ListarAsync(string? pesquisa = null);
    Task<int> GuardarAsync(ExercicioFinanceiroDto dto);
    Task AlternarAtivoAsync(int id, bool ativo);
    Task<FechoAnualPreviewDto> ObterPreviewFechoAsync(int id);
    Task EncerrarAsync(int id, int utilizadorId, string utilizadorNome);
    Task ReabrirAsync(int id, int utilizadorId, string utilizadorNome, string motivo);
    Task<IReadOnlyList<FechoAnualHistoricoDto>> ObterHistoricoFechoAsync(int id);
}
