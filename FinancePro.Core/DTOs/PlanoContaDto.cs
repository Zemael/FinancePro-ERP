using FinancePro.Core.Enums;

namespace FinancePro.Core.DTOs;

public sealed class PlanoContaDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public TipoConta Tipo { get; set; }
    public NaturezaContabil Natureza { get; set; }
    public bool AceitaLancamentos { get; set; }
    public bool CentroCustoObrigatorio { get; set; }
    public int? ContaPaiId { get; set; }
    public bool Ativo { get; set; }
    public string NomeCompleto => $"{Codigo} · {Nome}";
}
