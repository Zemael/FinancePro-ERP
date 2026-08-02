namespace FinancePro.Core.Entities;

/// <summary>Moeda disponível para empresas e operações financeiras.</summary>
public sealed class Moeda : EntityBase
{
    public string CodigoIso { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Simbolo { get; set; } = string.Empty;
    public int CasasDecimais { get; set; } = 0;
}
