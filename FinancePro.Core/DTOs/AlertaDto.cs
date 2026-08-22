namespace FinancePro.Core.DTOs;

public class AlertaDto
{
    public string Mensagem { get; set; } = string.Empty;
    public string Severidade { get; set; } = "Aviso"; // Aviso | Critico | Info
    public override string ToString() => Mensagem;
}
