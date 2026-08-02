using FinancePro.Core.DTOs;

namespace FinancePro.Services.Interfaces;

public interface IPermissaoService
{
    Task<IReadOnlyList<PermissaoPerfilDto>> ObterMatrizAsync(int perfilId);
    Task GuardarMatrizAsync(int perfilId, IReadOnlyCollection<PermissaoPerfilDto> permissoes);
    Task<IReadOnlyCollection<string>> ObterChavesAsync(int perfilId);
}
