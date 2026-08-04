namespace FinancePro.Core.DTOs;

public class FluxoCaixaMensalDto
{
    public string Periodo { get; set; } = string.Empty;
    public decimal Entradas { get; set; }
    public decimal Saidas { get; set; }
    public decimal Resultado => Entradas - Saidas;
}
