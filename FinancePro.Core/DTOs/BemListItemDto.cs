namespace FinancePro.Core.DTOs;

public class BemListItemDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string NumeroPatrimonial { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Serie { get; set; }
    public string? Localizacao { get; set; }
    public string? Responsavel { get; set; }
    public DateTime DataAquisicao { get; set; }
    public decimal ValorAquisicao { get; set; }
    public decimal ValorResidual { get; set; }
    public int VidaUtilAnos { get; set; }
    public string MetodoDepreciacao { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;

    /// <summary>Depreciação linear até à data de hoje, calculada ao vivo (não guardada).</summary>
    public decimal DepreciacaoAcumulada { get; set; }
    public decimal DepreciacaoMensal { get; set; }
    public decimal ValorLiquidoAtual { get; set; }
    public override string ToString() => string.IsNullOrWhiteSpace(Codigo) ? Descricao : $"{Codigo} · {Descricao}";
}
