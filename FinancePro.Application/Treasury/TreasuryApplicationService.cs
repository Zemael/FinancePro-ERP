using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;

namespace FinancePro.Application.Treasury;

public sealed class TreasuryApplicationService
{
    private readonly ITreasuryGateway _gateway;

    public TreasuryApplicationService(ITreasuryGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<Result<IReadOnlyList<MovimentoListItemDto>>> ListarMovimentosAsync(int empresaId, int maxRegistos = 100)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<MovimentoListItemDto>>.Fail("Empresa inválida.");
        if (maxRegistos <= 0) return Result<IReadOnlyList<MovimentoListItemDto>>.Fail("A quantidade de registos deve ser maior que zero.");
        try { return Result<IReadOnlyList<MovimentoListItemDto>>.Ok(await _gateway.ListarMovimentosAsync(empresaId, maxRegistos)); }
        catch (Exception ex) { return Result<IReadOnlyList<MovimentoListItemDto>>.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<OpcaoOrigemDto>>> ListarOrigensAsync(int empresaId)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<OpcaoOrigemDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<OpcaoOrigemDto>>.Ok(await _gateway.ListarOrigensAsync(empresaId)); }
        catch (Exception ex) { return Result<IReadOnlyList<OpcaoOrigemDto>>.Fail(ex.Message); }
    }

    public async Task<Result<IReadOnlyList<CategoriaOpcaoDto>>> ListarCategoriasAsync(int empresaId, TipoCategoria tipo)
    {
        if (empresaId <= 0) return Result<IReadOnlyList<CategoriaOpcaoDto>>.Fail("Empresa inválida.");
        try { return Result<IReadOnlyList<CategoriaOpcaoDto>>.Ok(await _gateway.ListarCategoriasAsync(empresaId, tipo)); }
        catch (Exception ex) { return Result<IReadOnlyList<CategoriaOpcaoDto>>.Fail(ex.Message); }
    }

    public async Task<Result<int>> RegistarMovimentoAsync(NovoMovimentoDto dto)
    {
        if (dto.EmpresaId <= 0) return Result<int>.Fail("Empresa inválida.");
        if (string.IsNullOrWhiteSpace(dto.Descricao)) return Result<int>.Fail("A descrição é obrigatória.");
        if (dto.Valor == 0) return Result<int>.Fail("O valor deve ser diferente de zero.");
        if (dto.CaixaId.HasValue == dto.ContaBancariaId.HasValue)
            return Result<int>.Fail("Selecione exatamente uma origem: caixa ou conta bancária.");

        try { return Result<int>.Ok(await _gateway.RegistarMovimentoAsync(dto), "Movimento registado com sucesso."); }
        catch (Exception ex) { return Result<int>.Fail(ex.Message); }
    }

    public async Task<Result> RegistarTransferenciaAsync(NovaTransferenciaDto dto)
    {
        if (dto.EmpresaId <= 0) return Result.Fail("Empresa inválida.");
        if (string.IsNullOrWhiteSpace(dto.Descricao)) return Result.Fail("A descrição é obrigatória.");
        if (dto.Valor <= 0) return Result.Fail("O valor deve ser maior que zero.");
        if (dto.OrigemId <= 0 || dto.DestinoId <= 0) return Result.Fail("Selecione a origem e o destino.");
        if (dto.OrigemTipo == dto.DestinoTipo && dto.OrigemId == dto.DestinoId)
            return Result.Fail("A origem e o destino não podem ser os mesmos.");

        try
        {
            await _gateway.RegistarTransferenciaAsync(dto);
            return Result.Ok("Transferência registada com sucesso.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> MarcarConciliadoAsync(int movimentoId, bool conciliado)
    {
        if (movimentoId <= 0) return Result.Fail("Movimento inválido.");
        try
        {
            await _gateway.MarcarConciliadoAsync(movimentoId, conciliado);
            return Result.Ok(conciliado ? "Movimento conciliado." : "Conciliação removida.");
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }
}
