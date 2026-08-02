namespace FinancePro.Core.DTOs;

public class NovoOrcamentoDto
{
    public int Ano { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string Moeda { get; set; } = "FCFA";
    public string? Observacoes { get; set; }
    public string? ElaboradoPor { get; set; }
    public int EmpresaId { get; set; }
}
