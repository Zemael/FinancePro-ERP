namespace FinancePro.Core.DTOs;

public class PesquisaResultadoDto
{
    public string Tipo { get; set; } = string.Empty; // Cliente | Fornecedor | Conta a Receber | Movimento
    public string Descricao { get; set; } = string.Empty;
    public string? Info { get; set; }
    public override string ToString() => Descricao;
}
