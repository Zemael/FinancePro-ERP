namespace FinancePro.Core.DTOs;

public class AtividadeRecenteDto
{
    public DateTime Data { get; set; }
    public string Utilizador { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}
