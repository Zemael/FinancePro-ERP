using FinancePro.Core.Enums;

namespace FinancePro.Core.Entities;

/// <summary>
/// Lançamento de tesouraria, sempre associado a exatamente uma origem:
/// uma Caixa OU uma Conta Bancária. O campo <see cref="Tipo"/> guarda o
/// sinal contabilístico (Receita soma, Despesa subtrai — usado pelo
/// Dashboard e pelo Orçamento); <see cref="TipoOperacao"/> guarda a
/// operação real escolhida pelo utilizador na Tesouraria (Entrada, Saída,
/// Transferência, Sangria, Reforço, Ajuste, Bloqueio).
///
/// Transferência/Sangria/Reforço geram duas linhas ligadas por
/// <see cref="GrupoTransferenciaId"/> (uma Saída na origem, uma Entrada no
/// destino) — nunca uma linha só, para o dinheiro nunca "desaparecer" de
/// um lado sem aparecer no outro.
/// </summary>
public class Movimento : EntityBase
{
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoCategoria Tipo { get; set; }
    public TipoOperacao TipoOperacao { get; set; } = TipoOperacao.Entrada;
    public EstadoMovimento Estado { get; set; } = EstadoMovimento.Confirmado;
    public bool Conciliado { get; set; }

    public string? FormaPagamento { get; set; }
    public string? CentroCusto { get; set; }

    /// <summary>Liga as duas linhas de uma Transferência/Sangria/Reforço entre si.</summary>
    public Guid? GrupoTransferenciaId { get; set; }

    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public int? CaixaId { get; set; }
    public Caixa? Caixa { get; set; }

    public int? ContaBancariaId { get; set; }
    public ContaBancaria? ContaBancaria { get; set; }

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
