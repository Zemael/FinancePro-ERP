namespace FinancePro.Core.DTOs;

public class NovaRevisaoOrcamentalDto
{
    public int OrcamentoId { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;
}
