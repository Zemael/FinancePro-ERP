using FinancePro.Core.Enums;

namespace FinancePro.Core.DTOs;

public class NovoBemDto
{
    public string NumeroPatrimonial { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Serie { get; set; }
    public string? Localizacao { get; set; }
    public string? Responsavel { get; set; }
    public DateTime DataAquisicao { get; set; } = DateTime.Today;
    public decimal ValorAquisicao { get; set; }
    public decimal ValorResidual { get; set; }
    public int VidaUtilAnos { get; set; }
    public MetodoDepreciacao MetodoDepreciacao { get; set; } = MetodoDepreciacao.Linear;
    public int EmpresaId { get; set; }
}
