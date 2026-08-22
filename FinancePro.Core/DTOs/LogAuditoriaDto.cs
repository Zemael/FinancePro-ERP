namespace FinancePro.Core.DTOs;

public class LogAuditoriaDto
{
    public DateTime Data { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string? Detalhe { get; set; }
    public string UtilizadorNome { get; set; } = string.Empty;
    public override string ToString() => string.IsNullOrWhiteSpace(Acao) ? Detalhe ?? string.Empty : Acao;
}
