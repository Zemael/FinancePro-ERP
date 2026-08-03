using FinancePro.Core.DTOs;

namespace FinancePro.Application.Receivables;

public sealed record ReceivablesQuery(
    int EmpresaId,
    string? Search = null,
    string? Status = null,
    DateTime? DueFrom = null,
    DateTime? DueTo = null);

public sealed record ReceivablesSummary(
    decimal OpenAmount,
    decimal OverdueAmount,
    decimal DueTodayAmount,
    decimal ReceivedAmount,
    int OpenCount,
    int OverdueCount);

public sealed record ReceivablesDashboard(
    IReadOnlyList<ContaReceberListItemDto> Items,
    IReadOnlyList<ClienteOpcaoDto> Customers,
    IReadOnlyList<CategoriaOpcaoDto> Categories,
    IReadOnlyList<OpcaoOrigemDto> Origins,
    ReceivablesSummary Summary);
