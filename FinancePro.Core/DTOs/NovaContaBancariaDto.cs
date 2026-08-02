namespace FinancePro.Core.DTOs;

public class NovaContaBancariaDto
{
    public string NumeroConta { get; set; } = string.Empty;
    public string? IBAN { get; set; }
    public string Titular { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public string Moeda { get; set; } = "FCFA";
    public int BancoId { get; set; }
    public int EmpresaId { get; set; }
}
