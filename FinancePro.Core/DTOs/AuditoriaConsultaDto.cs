namespace FinancePro.Core.DTOs;

public sealed class AuditoriaConsultaDto
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public string Entidade { get; set; } = string.Empty;
    public int RegistoId { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string? Detalhe { get; set; }
    public int UtilizadorId { get; set; }
    public string UtilizadorNome { get; set; } = string.Empty;
    public override string ToString() => string.IsNullOrWhiteSpace(Acao) ? Entidade : $"{Entidade} · {Acao}";
}
