using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Assets;

public sealed class AssetApplicationService
{
    private readonly IAssetGateway _gateway;

    public AssetApplicationService(IAssetGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<Result<IReadOnlyList<BemListItemDto>>> ListarAsync(int empresaId)
    {
        if (empresaId <= 0)
            return Result<IReadOnlyList<BemListItemDto>>.Fail("Empresa inválida.");

        try
        {
            return Result<IReadOnlyList<BemListItemDto>>.Ok(await _gateway.ListarAsync(empresaId));
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<BemListItemDto>>.Fail(ex.Message);
        }
    }

    public async Task<Result<int>> GuardarAsync(
        int? bemId,
        NovoBemDto dto,
        int utilizadorId,
        string utilizadorNome)
    {
        var erros = Validar(dto);
        if (erros.Count > 0)
            return Result<int>.Fail(erros);

        if (utilizadorId <= 0)
            return Result<int>.Fail("Utilizador inválido.");

        try
        {
            if (bemId.HasValue)
            {
                if (bemId.Value <= 0)
                    return Result<int>.Fail("Bem inválido.");

                await _gateway.AtualizarAsync(bemId.Value, dto, utilizadorId, utilizadorNome);
                return Result<int>.Ok(bemId.Value, "Bem atualizado com sucesso.");
            }

            var novoId = await _gateway.CriarAsync(dto, utilizadorId, utilizadorNome);
            return Result<int>.Ok(novoId, "Bem criado com sucesso.");
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex.Message);
        }
    }

    public async Task<Result> AbaterAsync(int bemId, int utilizadorId, string utilizadorNome)
    {
        if (bemId <= 0)
            return Result.Fail("Bem inválido.");
        if (utilizadorId <= 0)
            return Result.Fail("Utilizador inválido.");

        try
        {
            await _gateway.AbaterAsync(bemId, utilizadorId, utilizadorNome);
            return Result.Ok("Bem abatido/desativado com sucesso.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    private static IReadOnlyList<string> Validar(NovoBemDto dto)
    {
        var erros = new List<string>();

        if (dto.EmpresaId <= 0) erros.Add("Empresa inválida.");
        if (string.IsNullOrWhiteSpace(dto.NumeroPatrimonial)) erros.Add("O número patrimonial é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Descricao)) erros.Add("A descrição é obrigatória.");
        if (dto.DataAquisicao == default) erros.Add("A data de aquisição é obrigatória.");
        if (dto.DataAquisicao.Date > DateTime.Today) erros.Add("A data de aquisição não pode estar no futuro.");
        if (dto.ValorAquisicao < 0) erros.Add("O valor de aquisição não pode ser negativo.");
        if (dto.VidaUtilAnos < 0) erros.Add("A vida útil não pode ser negativa.");

        return erros;
    }
}
