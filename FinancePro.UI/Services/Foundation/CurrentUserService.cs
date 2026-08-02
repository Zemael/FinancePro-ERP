using FinancePro.Application.Common.Interfaces;
using FinancePro.UI.Common;

namespace FinancePro.UI.Services.Foundation;

public sealed class CurrentUserService : ICurrentUserService
{
    public bool IsAuthenticated => SessaoAtual.UtilizadorId > 0;
    public int UserId => SessaoAtual.UtilizadorId;
    public int CompanyId => SessaoAtual.EmpresaId;
    public string FullName => SessaoAtual.NomeCompleto;
    public string ProfileName => SessaoAtual.PerfilNome;
    public bool HasPermission(string module, string action = "Consultar") => SessaoAtual.TemPermissao(module, action);
}
