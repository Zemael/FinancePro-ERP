namespace FinancePro.Core.DTOs;

/// <summary>Linha de movimento recente mostrada no Dashboard.</summary>
public class MovimentoRecenteDto
{
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // "Receita" | "Despesa"
    public decimal Valor { get; set; }
}
