using FinancePro.Core.Enums;

namespace FinancePro.Core.DTOs;

public class NovaTransferenciaDto
{
    public DateTime Data { get; set; } = DateTime.Today;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoOperacao TipoOperacao { get; set; } = TipoOperacao.Transferencia;
    public string? FormaPagamento { get; set; }
    public string? CentroCusto { get; set; }

    public string OrigemTipo { get; set; } = string.Empty;
    public int OrigemId { get; set; }
    public string DestinoTipo { get; set; } = string.Empty;
    public int DestinoId { get; set; }

    public int EmpresaId { get; set; }
}
