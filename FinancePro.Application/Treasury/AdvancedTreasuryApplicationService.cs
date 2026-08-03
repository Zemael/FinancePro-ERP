using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;

namespace FinancePro.Application.Treasury;

public sealed class AdvancedTreasuryApplicationService
{
    private readonly IAdvancedTreasuryGateway _gateway;

    public AdvancedTreasuryApplicationService(IAdvancedTreasuryGateway gateway) => _gateway = gateway;

    public async Task<Result<TreasuryWorkspace>> LoadAsync(int empresaId, TipoOperacao operation, CancellationToken cancellationToken = default)
    {
        if (empresaId <= 0) return Result<TreasuryWorkspace>.Fail("A empresa é obrigatória.");
        try
        {
            var type = operation == TipoOperacao.Saida ? TipoCategoria.Despesa : TipoCategoria.Receita;
            var movements = await _gateway.ListMovementsAsync(empresaId, cancellationToken: cancellationToken);
            var origins = await _gateway.ListOriginsAsync(empresaId, cancellationToken);
            var categories = await _gateway.ListCategoriesAsync(empresaId, type, cancellationToken);
            return Result<TreasuryWorkspace>.Ok(new TreasuryWorkspace(movements, origins, categories));
        }
        catch (Exception ex)
        {
            return Result<TreasuryWorkspace>.Fail(ex.Message, "Não foi possível carregar a Tesouraria.");
        }
    }

    public async Task<Result> RegisterMovementAsync(NovoMovimentoDto request, CancellationToken cancellationToken = default)
    {
        var errors = ValidateMovement(request);
        if (errors.Count > 0) return Result.Fail(errors);
        try
        {
            request.Descricao = request.Descricao.Trim();
            request.FormaPagamento = Normalize(request.FormaPagamento);
            request.CentroCusto = Normalize(request.CentroCusto);
            await _gateway.RegisterMovementAsync(request, cancellationToken);
            return Result.Ok("Movimento registado com sucesso.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message, "Não foi possível registar o movimento.");
        }
    }

    public async Task<Result> RegisterTransferAsync(NovaTransferenciaDto request, CancellationToken cancellationToken = default)
    {
        var errors = ValidateTransfer(request);
        if (errors.Count > 0) return Result.Fail(errors);
        try
        {
            request.Descricao = request.Descricao.Trim();
            request.FormaPagamento = Normalize(request.FormaPagamento);
            request.CentroCusto = Normalize(request.CentroCusto);
            await _gateway.RegisterTransferAsync(request, cancellationToken);
            return Result.Ok(request.TipoOperacao switch
            {
                TipoOperacao.Reforco => "Reforço registado com sucesso.",
                TipoOperacao.Sangria => "Sangria registada com sucesso.",
                _ => "Transferência registada com sucesso."
            });
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message, "Não foi possível concluir a operação de Tesouraria.");
        }
    }

    public async Task<Result> SetReconciledAsync(int movementId, bool reconciled, CancellationToken cancellationToken = default)
    {
        if (movementId <= 0) return Result.Fail("Selecione um movimento válido.");
        try
        {
            await _gateway.SetReconciledAsync(movementId, reconciled, cancellationToken);
            return Result.Ok(reconciled ? "Movimento conciliado." : "Conciliação removida.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message, "Não foi possível atualizar a conciliação.");
        }
    }

    private static IReadOnlyList<string> ValidateMovement(NovoMovimentoDto request)
    {
        var errors = new List<string>();
        if (request.EmpresaId <= 0) errors.Add("A empresa é obrigatória.");
        if (string.IsNullOrWhiteSpace(request.Descricao)) errors.Add("Indique uma descrição.");
        if (request.Valor == 0) errors.Add("O valor deve ser diferente de zero.");
        if (request.CaixaId.HasValue == request.ContaBancariaId.HasValue) errors.Add("Selecione exatamente uma origem: caixa ou conta bancária.");
        if (request.Data == default) errors.Add("A data é obrigatória.");
        return errors;
    }

    private static IReadOnlyList<string> ValidateTransfer(NovaTransferenciaDto request)
    {
        var errors = new List<string>();
        if (request.EmpresaId <= 0) errors.Add("A empresa é obrigatória.");
        if (string.IsNullOrWhiteSpace(request.Descricao)) errors.Add("Indique uma descrição.");
        if (request.Valor <= 0) errors.Add("O valor deve ser superior a zero.");
        if (request.Data == default) errors.Add("A data é obrigatória.");
        if (request.OrigemId <= 0 || request.DestinoId <= 0) errors.Add("Selecione a origem e o destino.");
        if (request.OrigemTipo == request.DestinoTipo && request.OrigemId == request.DestinoId) errors.Add("A origem e o destino não podem ser iguais.");
        return errors;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record TreasuryWorkspace(
    IReadOnlyList<MovimentoListItemDto> Movements,
    IReadOnlyList<OpcaoOrigemDto> Origins,
    IReadOnlyList<CategoriaOpcaoDto> Categories);
