using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.Common;

public sealed class DtoDisplayTests
{
    [Fact]
    public void EmpresaListItemDto_ToString_ReturnsCompanyName()
    {
        var empresa = new EmpresaListItemDto { Nome = "FinancePro SARL" };

        Assert.Equal("FinancePro SARL", empresa.ToString());
    }

    [Fact]
    public void PerfilDto_ToString_ReturnsProfileName()
    {
        var perfil = new PerfilDto { Nome = "Administrador" };

        Assert.Equal("Administrador", perfil.ToString());
    }

    [Fact]
    public void VisualDtos_MustOverrideDefaultObjectText()
    {
        var explicitVisualTypes = new HashSet<string>
        {
            nameof(AlertaDto), nameof(AuditoriaConsultaDto), nameof(BusinessPartnerDto),
            nameof(DatabaseBackupDto), nameof(DocumentoFiscalDto), nameof(DocumentoItemDto),
            nameof(EmpresaDto), nameof(ExercicioFinanceiroDto), nameof(ExecucaoMensalDto),
            nameof(LogAuditoriaDto), nameof(MoedaDto), nameof(MovimentoStockDto),
            nameof(OpcaoOrigemDto), nameof(PerfilDto), nameof(PermissaoPerfilDto),
            nameof(PesquisaResultadoDto), nameof(ProdutoRentabilidadeDto), nameof(ProdutoStockDto),
            nameof(RevisaoOrcamentalDto), nameof(SaldoOrigemDto), nameof(SessaoCaixaDto),
            nameof(TreasuryAgingDto), nameof(TreasuryForecastItemDto), nameof(UtilizadorDto)
        };

        var types = typeof(EmpresaDto).Assembly.GetTypes()
            .Where(type => type.Namespace == "FinancePro.Core.DTOs" && !type.IsAbstract)
            .Where(type => type.Name.EndsWith("ListItemDto", StringComparison.Ordinal) ||
                           type.Name.EndsWith("OpcaoDto", StringComparison.Ordinal) ||
                           explicitVisualTypes.Contains(type.Name));

        foreach (var type in types)
        {
            var method = type.GetMethod(nameof(ToString), Type.EmptyTypes);
            Assert.NotNull(method);
            Assert.NotEqual(typeof(object), method!.DeclaringType);
        }
    }
}
