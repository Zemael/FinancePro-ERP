using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Revenue;

public sealed class RevenueApplicationService
{
    private readonly IRevenueGateway _gateway;

    public RevenueApplicationService(IRevenueGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<Result<IReadOnlyList<ContaReceberListItemDto>>> ListarAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<ContaReceberListItemDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<ContaReceberListItemDto>>.Ok(await _gateway.ListarAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<ContaReceberListItemDto>>.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<ClienteOpcaoDto>>> ListarClientesAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<ClienteOpcaoDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<ClienteOpcaoDto>>.Ok(await _gateway.ListarClientesAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<ClienteOpcaoDto>>.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<CategoriaOpcaoDto>>> ListarCategoriasAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<CategoriaOpcaoDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<CategoriaOpcaoDto>>.Ok(await _gateway.ListarCategoriasAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<CategoriaOpcaoDto>>.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<OpcaoOrigemDto>>> ListarOrigensAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<OpcaoOrigemDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<OpcaoOrigemDto>>.Ok(await _gateway.ListarOrigensAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<OpcaoOrigemDto>>.Fail(ex.Message); }
    }

    public async Task<Result> CriarAsync(NovaContaReceberDto dto)
    {
        var erros = Validar(dto);
        if (erros.Count > 0) return Result.Fail(erros);

        try
        {
            await _gateway.CriarAsync(dto);
            return Result.Ok("Conta a receber registada com sucesso.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> RegistarRecebimentoAsync(int contaReceberId, string origemTipo, int origemId, DateTime dataRecebimento)
    {
        if (contaReceberId <= 0) return Result.Fail("Conta a receber inválida.");
        if (origemId <= 0) return Result.Fail("Selecione a origem do recebimento.");
        if (origemTipo is not ("Caixa" or "ContaBancaria")) return Result.Fail("Origem de recebimento inválida.");
        if (dataRecebimento == default) return Result.Fail("Indique a data do recebimento.");

        try
        {
            await _gateway.RegistarRecebimentoAsync(contaReceberId, origemTipo, origemId, dataRecebimento);
            return Result.Ok("Recebimento confirmado e lançado na Tesouraria.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> CancelarAsync(int contaReceberId)
    {
        if (contaReceberId <= 0) return Result.Fail("Conta a receber inválida.");
        try
        {
            await _gateway.CancelarAsync(contaReceberId);
            return Result.Ok("Conta a receber cancelada.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    private static IReadOnlyList<string> Validar(NovaContaReceberDto dto)
    {
        var erros = new List<string>();
        if (dto.EmpresaId <= 0) erros.Add("Empresa inválida.");
        if (string.IsNullOrWhiteSpace(dto.Descricao)) erros.Add("A descrição é obrigatória.");
        if (dto.Valor <= 0) erros.Add("O valor tem de ser maior do que zero.");
        if (dto.DataEmissao == default) erros.Add("A data de emissão é obrigatória.");
        if (dto.DataVencimento == default) erros.Add("A data de vencimento é obrigatória.");
        if (dto.DataVencimento.Date < dto.DataEmissao.Date) erros.Add("A data de vencimento não pode ser anterior à data de emissão.");
        return erros;
    }
}
