namespace FinancePro.Application.Suppliers;

public sealed record SupplierSaveRequest(
    int Id,
    int EmpresaId,
    string Nome,
    string? Nif,
    string? Telefone,
    string? Email,
    string? Morada,
    bool Ativo = true);
