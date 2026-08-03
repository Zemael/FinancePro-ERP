namespace FinancePro.Application.Customers;

public sealed record CustomerSaveRequest(
    int Id,
    int EmpresaId,
    string Nome,
    string? Nif,
    string? Telefone,
    string? Email,
    string? Morada,
    bool Ativo = true);
