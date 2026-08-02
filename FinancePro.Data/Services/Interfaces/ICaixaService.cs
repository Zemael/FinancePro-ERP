using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface ICaixaService
{
    Task<IReadOnlyList<CaixaListItemDto>> ListarAsync(int empresaId);
    Task<int> CriarAsync(NovoCaixaDto dto);
    Task AtualizarAsync(int caixaId, NovoCaixaDto dto);
    Task AlternarAtivoAsync(int caixaId, bool ativo);

    Task<SessaoCaixaDto?> ObterSessaoAbertaAsync(int empresaId, int? caixaId = null);
    Task<SessaoCaixaDto> AbrirAsync(AbrirCaixaDto dto);
    Task<SessaoCaixaDto> FecharAsync(FecharCaixaDto dto);
}
