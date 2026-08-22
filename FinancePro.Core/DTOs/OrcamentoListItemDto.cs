namespace FinancePro.Core.DTOs;

public class OrcamentoListItemDto
{
    public int Id { get; set; }
    public int Ano { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string Moeda { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime? DataAprovacao { get; set; }
    public override string ToString() => string.IsNullOrWhiteSpace(Nome) ? Ano.ToString() : $"{Ano} · {Nome}";
}
