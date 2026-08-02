namespace FinancePro.Core.DTOs;

public class ContaBancariaListItemDto
{
    public int Id { get; set; }
    public string NumeroConta { get; set; } = string.Empty;
    public string? IBAN { get; set; }
    public string Titular { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public string Moeda { get; set; } = string.Empty;
    public string BancoNome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}
