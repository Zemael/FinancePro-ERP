namespace FinancePro.Application.MasterData.Partners;

public sealed record BusinessPartnerSaveRequest(
    int Id,
    int EmpresaId,
    string Tipo,
    string Nome,
    string? NIF,
    string? Telefone,
    string? Email,
    string? Morada);
