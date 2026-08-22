namespace FinancePro.Core.DTOs;

public class OpcaoOrigemDto
{
    public string Tipo { get; set; } = string.Empty; // "Caixa" | "ContaBancaria"
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Disponivel { get; set; } = true;
    public string? MotivoIndisponibilidade { get; set; }
    public override string ToString() => Nome;
}
