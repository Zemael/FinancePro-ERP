namespace FinancePro.Core.DTOs;

public class SaldoOrigemDto
{
    public string Tipo { get; set; } = string.Empty; // "Caixa" | "Banco"
    public string Nome { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
}
