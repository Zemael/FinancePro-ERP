using FinancePro.Core.Enums;

namespace FinancePro.Core.Entities;

public class LancamentoContabil : EntityBase
{
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public DateTime DataLancamento { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? DocumentoReferencia { get; set; }
    public string? OrigemModulo { get; set; }
    public EstadoLancamentoContabil Estado { get; set; } = EstadoLancamentoContabil.Rascunho;
    public DateTime? DataContabilizacao { get; set; }
    public int? UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
    public ICollection<LancamentoContabilLinha> Linhas { get; set; } = new List<LancamentoContabilLinha>();
}
