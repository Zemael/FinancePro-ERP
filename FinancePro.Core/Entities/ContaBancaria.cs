namespace FinancePro.Core.Entities;

/// <summary>Conta bancária de uma empresa, associada a um banco.</summary>
public class ContaBancaria : EntityBase
{
    public string NumeroConta { get; set; } = string.Empty;
    public string? IBAN { get; set; }
    public string Titular { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public string Moeda { get; set; } = "FCFA";

    public int BancoId { get; set; }
    public Banco Banco { get; set; } = null!;

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
