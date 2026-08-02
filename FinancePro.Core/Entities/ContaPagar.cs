using FinancePro.Core.Enums;

namespace FinancePro.Core.Entities;

/// <summary>
/// Conta a pagar (fatura/valor devido a um fornecedor). Quando paga,
/// gera automaticamente um Movimento de Tesouraria do tipo Despesa.
/// </summary>
public class ContaPagar : EntityBase
{
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataEmissao { get; set; }
    public DateTime DataVencimento { get; set; }
    public EstadoConta Estado { get; set; } = EstadoConta.Pendente;
    public DateTime? DataPagamento { get; set; }

    public string? FormaPagamento { get; set; }
    public string? CentroCusto { get; set; }

    public int? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }

    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public int? MovimentoId { get; set; }
    public Movimento? Movimento { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
