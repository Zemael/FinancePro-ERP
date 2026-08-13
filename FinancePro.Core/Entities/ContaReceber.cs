using FinancePro.Core.Enums;

namespace FinancePro.Core.Entities;

/// <summary>
/// Conta a receber (fatura/valor esperado de um cliente). Quando recebida,
/// gera automaticamente um Movimento de Tesouraria do tipo Receita.
/// </summary>
public class ContaReceber : EntityBase
{
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal ValorLiquidado { get; set; }
    public string ComercialEstado { get; set; } = "Faturada";
    public string? NumeroProposta { get; set; }
    public string? NumeroFatura { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataFaturacao { get; set; }
    public DateTime DataEmissao { get; set; }
    public DateTime DataVencimento { get; set; }
    public EstadoConta Estado { get; set; } = EstadoConta.Pendente;
    public DateTime? DataRecebimento { get; set; }

    public string? FormaPagamento { get; set; }
    public string? CentroCusto { get; set; }

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public int? MovimentoId { get; set; }
    public Movimento? Movimento { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
