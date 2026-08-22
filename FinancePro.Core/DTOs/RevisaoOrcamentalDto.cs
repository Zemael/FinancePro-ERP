namespace FinancePro.Core.DTOs;

public class RevisaoOrcamentalDto
{
    public int Id { get; set; }
    public int Versao { get; set; }
    public DateTime Data { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public override string ToString() => $"Versão {Versao} · {Motivo}";
}
