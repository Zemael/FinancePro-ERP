namespace FinancePro.Core.DTOs;

public sealed class DocumentoFiscalDto
{
    public int Id { get; set; }
    public int ContaReceberId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public DateTime DataEmissao { get; set; }
    public decimal BaseTributavel { get; set; }
    public decimal ValorIva { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Emitido";
    public string? DocumentoOrigem { get; set; }
    public string? Motivo { get; set; }
}

public sealed class NovaNotaFiscalDto
{
    public int ContaReceberId { get; set; }
    public string Tipo { get; set; } = "Credito";
    public decimal Valor { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
