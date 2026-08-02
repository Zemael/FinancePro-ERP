namespace FinancePro.Core.DTOs;

public class ExecucaoMensalDto
{
    public int Mes { get; set; }
    public string MesNome { get; set; } = string.Empty;
    public decimal PrevistoReceitas { get; set; }
    public decimal RealizadoReceitas { get; set; }
    public decimal PrevistoDespesas { get; set; }
    public decimal RealizadoDespesas { get; set; }
}
