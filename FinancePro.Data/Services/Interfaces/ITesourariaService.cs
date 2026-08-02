using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;

namespace FinancePro.Services.Interfaces;

public interface ITesourariaService
{
    Task<IReadOnlyList<MovimentoListItemDto>> ListarMovimentosAsync(int empresaId, int maxRegistos = 100);
    Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId);
    Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId, TipoCategoria tipo);
    Task<int> RegistarMovimentoAsync(NovoMovimentoDto dto);
    Task RegistarTransferenciaAsync(NovaTransferenciaDto dto);
    Task MarcarConciliadoAsync(int movimentoId, bool conciliado);
}
