using FinancePro.Core.DTOs;

namespace FinancePro.Application.Accounting;

public interface IAccountingGateway
{
    Task<IReadOnlyList<PlanoContaDto>> ListAccountsAsync(int empresaId, string? pesquisa, CancellationToken cancellationToken = default);
    Task<bool> AccountCodeExistsAsync(int empresaId, string codigo, int ignoreId, CancellationToken cancellationToken = default);
    Task<int> SaveAccountAsync(AccountSaveRequest request, CancellationToken cancellationToken = default);
    Task SetAccountActiveAsync(int id, bool ativo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LancamentoContabilDto>> ListEntriesAsync(int empresaId, CancellationToken cancellationToken = default);
    Task<int> SaveEntryAsync(JournalEntryRequest request, CancellationToken cancellationToken = default);
    Task PostEntryAsync(int id, CancellationToken cancellationToken = default);
}
