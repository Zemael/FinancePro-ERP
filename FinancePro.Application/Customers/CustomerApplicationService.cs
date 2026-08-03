using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Customers;

public sealed class CustomerApplicationService
{
    private readonly ICustomerGateway _gateway;

    public CustomerApplicationService(ICustomerGateway gateway) => _gateway = gateway;

    public async Task<Result<IReadOnlyList<ClienteDto>>> ListAsync(int empresaId, string? pesquisa = null, CancellationToken cancellationToken = default)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<ClienteDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<ClienteDto>>.Ok(await _gateway.ListAsync(empresaId, pesquisa?.Trim(), cancellationToken)); }
        catch (Exception ex) { return Result<IReadOnlyList<ClienteDto>>.Fail(ex.Message, "Não foi possível carregar os clientes."); }
    }

    public async Task<Result<int>> SaveAsync(CustomerSaveRequest request, CancellationToken cancellationToken = default)
    {
        var errors = Validate(request);
        if (errors.Count > 0) return Result<int>.Fail(errors);
        try
        {
            var nif = Normalize(request.Nif);
            if (!string.IsNullOrWhiteSpace(nif) && await _gateway.ExistsWithNifAsync(request.EmpresaId, nif, request.Id, cancellationToken))
                return Result<int>.Fail("Já existe um cliente com este NIF.");

            var normalized = request with
            {
                Nome = request.Nome.Trim(), Nif = nif, Telefone = Normalize(request.Telefone),
                Email = NormalizeEmail(request.Email), Morada = Normalize(request.Morada)
            };
            var id = await _gateway.SaveAsync(normalized, cancellationToken);
            return Result<int>.Ok(id, request.Id == 0 ? "Cliente criado com sucesso." : "Cliente atualizado com sucesso.");
        }
        catch (Exception ex) { return Result<int>.Fail(ex.Message, "Não foi possível guardar o cliente."); }
    }

    public async Task<Result> SetActiveAsync(int id, bool ativo, CancellationToken cancellationToken = default)
    {
        if (id <= 0) return Result.Fail("Cliente inválido.");
        try { await _gateway.SetActiveAsync(id, ativo, cancellationToken); return Result.Ok(ativo ? "Cliente ativado." : "Cliente desativado."); }
        catch (Exception ex) { return Result.Fail(ex.Message, "Não foi possível alterar o estado do cliente."); }
    }

    private static List<string> Validate(CustomerSaveRequest r)
    {
        var e = new List<string>();
        if (r.EmpresaId <= 0) e.Add("Empresa inválida.");
        if (string.IsNullOrWhiteSpace(r.Nome)) e.Add("Informe o nome do cliente.");
        else if (r.Nome.Trim().Length > 150) e.Add("O nome deve ter no máximo 150 caracteres.");
        if (!string.IsNullOrWhiteSpace(r.Email) && !r.Email.Contains('@')) e.Add("Informe um email válido.");
        return e;
    }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? NormalizeEmail(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
