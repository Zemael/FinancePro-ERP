using FinancePro.Application.Accounting;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Platform;

public sealed record DocumentNumberRequest(
    int EmpresaId,
    string Modulo,
    string Prefixo,
    DateTime DataDocumento,
    int Digitos = 6,
    bool ReiniciarAnualmente = true);

public sealed record AuditTrailRequest(
    int EmpresaId,
    int UtilizadorId,
    string UtilizadorNome,
    string Modulo,
    string Entidade,
    int RegistoId,
    string Operacao,
    string? Detalhe = null);

public sealed record FinancialOperationRequest(
    int EmpresaId,
    string Modulo,
    string Prefixo,
    string Descricao,
    NovoMovimentoDto Movimento,
    JournalEntryRequest Lancamento,
    int UtilizadorId,
    string UtilizadorNome,
    bool ContabilizarAutomaticamente = true);

public sealed record FinancialOperationResult(
    string NumeroDocumento,
    int LancamentoContabilId,
    bool Contabilizado);
