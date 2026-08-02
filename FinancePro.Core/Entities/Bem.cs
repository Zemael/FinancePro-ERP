using FinancePro.Core.Enums;

namespace FinancePro.Core.Entities;

/// <summary>Bem patrimonial (imobilizado) — Módulo 08, Gestão Patrimonial.</summary>
public class Bem : EntityBase
{
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
    public int VidaUtilAnos { get; set; }
    public MetodoDepreciacao MetodoDepreciacao { get; set; } = MetodoDepreciacao.Linear;

    public EstadoBem Estado { get; set; } = EstadoBem.Ativo;

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
