using FinancePro.Application.Treasury;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using FinancePro.Services.Interfaces;

namespace FinancePro.Data.Treasury;

public sealed class TreasuryGateway : ITreasuryGateway
{
    private readonly ITesourariaService _service;

    public TreasuryGateway(ITesourariaService service)
    {
        _service = service;
    }

    public Task<IReadOnlyList<MovimentoListItemDto>> ListarMovimentosAsync(int empresaId, int maxRegistos = 100) =>
        _service.ListarMovimentosAsync(empresaId, maxRegistos);

    public Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId) =>
        _service.ListarOrigensAsync(empresaId);

    public Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId, TipoCategoria tipo) =>
        _service.ListarCategoriasAsync(empresaId, tipo);

    public Task<int> RegistarMovimentoAsync(NovoMovimentoDto dto) =>
        _service.RegistarMovimentoAsync(dto);

    public Task RegistarTransferenciaAsync(NovaTransferenciaDto dto) =>
        _service.RegistarTransferenciaAsync(dto);

    public Task MarcarConciliadoAsync(int movimentoId, bool conciliado) =>
        _service.MarcarConciliadoAsync(movimentoId, conciliado);

    public Task<TreasuryOverviewDto> ObterResumoAsync(int empresaId) =>
        _service.ObterResumoAsync(empresaId);

    public Task<IReadOnlyList<TreasuryForecastItemDto>> ListarPrevisaoAsync(int empresaId, int dias = 30) =>
        _service.ListarPrevisaoAsync(empresaId, dias);
}
