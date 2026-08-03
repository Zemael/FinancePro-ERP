using FinancePro.Core.Enums;

namespace FinancePro.Application.Accounting;

public sealed record AccountSaveRequest(int Id, int EmpresaId, string Codigo, string Nome, TipoConta Tipo, NaturezaContabil Natureza, bool AceitaLancamentos, bool CentroCustoObrigatorio, int? ContaPaiId, bool Ativo);
public sealed record JournalLineRequest(int PlanoContasId, string? Descricao, decimal Debito, decimal Credito, string? CentroCusto);
public sealed record JournalEntryRequest(int EmpresaId, DateTime DataLancamento, string Descricao, string? DocumentoReferencia, string? OrigemModulo, int? UtilizadorId, IReadOnlyList<JournalLineRequest> Linhas);
