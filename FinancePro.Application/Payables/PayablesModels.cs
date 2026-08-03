using FinancePro.Core.DTOs;

namespace FinancePro.Application.Payables;

public sealed record PayablesQuery(
    int EmpresaId,
    string? Search = null,
    string? Status = null,
    DateTime? DueFrom = null,
    DateTime? DueTo = null);

public sealed record PayablesSummary(
    decimal OpenAmount,
    decimal OverdueAmount,
    decimal DueTodayAmount,
    decimal PaidAmount,
    int OpenCount,
    int OverdueCount);

public sealed record PayablesDashboard(
    IReadOnlyList<ContaPagarListItemDto> Items,
    IReadOnlyList<FornecedorOpcaoDto> Suppliers,
    IReadOnlyList<CategoriaOpcaoDto> Categories,
    IReadOnlyList<OpcaoOrigemDto> Origins,
    PayablesSummary Summary);
