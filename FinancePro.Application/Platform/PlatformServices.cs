using FinancePro.Application.Accounting;
using FinancePro.Application.Common.Results;
using FinancePro.Application.Treasury;
using Microsoft.Extensions.DependencyInjection;

namespace FinancePro.Application.Platform;

public sealed class NumberingService : INumberingService
{
    private readonly INumberingGateway _gateway;
    public NumberingService(INumberingGateway gateway) => _gateway = gateway;

    public Task<string> GetNextAsync(DocumentNumberRequest request, CancellationToken cancellationToken = default)
    {
        if (request.EmpresaId <= 0) throw new ArgumentException("Empresa inválida.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Modulo)) throw new ArgumentException("Módulo obrigatório.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Prefixo)) throw new ArgumentException("Prefixo obrigatório.", nameof(request));
        if (request.Digitos is < 3 or > 12) throw new ArgumentOutOfRangeException(nameof(request), "Os dígitos devem estar entre 3 e 12.");
        return _gateway.GetNextAsync(request with
        {
            Modulo = request.Modulo.Trim().ToUpperInvariant(),
            Prefixo = request.Prefixo.Trim().ToUpperInvariant()
        }, cancellationToken);
    }
}

public sealed class AuditTrailService : IAuditTrailService
{
    private readonly IAuditTrailGateway _gateway;
    public AuditTrailService(IAuditTrailGateway gateway) => _gateway = gateway;

    public Task WriteAsync(AuditTrailRequest request, CancellationToken cancellationToken = default)
    {
        if (request.EmpresaId <= 0 || request.UtilizadorId <= 0) return Task.CompletedTask;
        return _gateway.WriteAsync(request with
        {
            Modulo = request.Modulo.Trim(),
            Entidade = request.Entidade.Trim(),
            Operacao = request.Operacao.Trim()
        }, cancellationToken);
    }
}

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    public DomainEventDispatcher(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        foreach (var handler in _serviceProvider.GetServices<IDomainEventHandler<TEvent>>())
            await handler.HandleAsync(domainEvent, cancellationToken);
    }
}

public sealed class FinancialEngineService : IFinancialEngine
{
    private readonly INumberingService _numbering;
    private readonly AdvancedTreasuryApplicationService _treasury;
    private readonly AccountingApplicationService _accounting;
    private readonly IAuditTrailService _audit;
    private readonly IDomainEventDispatcher _events;

    public FinancialEngineService(
        INumberingService numbering,
        AdvancedTreasuryApplicationService treasury,
        AccountingApplicationService accounting,
        IAuditTrailService audit,
        IDomainEventDispatcher events)
    {
        _numbering = numbering;
        _treasury = treasury;
        _accounting = accounting;
        _audit = audit;
        _events = events;
    }

    public async Task<Result<FinancialOperationResult>> ProcessAsync(
        FinancialOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = Validate(request);
        if (errors.Count > 0) return Result<FinancialOperationResult>.Fail(errors);

        try
        {
            var documentNumber = await _numbering.GetNextAsync(new DocumentNumberRequest(
                request.EmpresaId,
                request.Modulo,
                request.Prefixo,
                request.Movimento.Data), cancellationToken);

            request.Movimento.Descricao = $"{documentNumber} - {request.Descricao.Trim()}";
            var treasuryResult = await _treasury.RegisterMovementAsync(request.Movimento, cancellationToken);
            if (treasuryResult.IsFailure)
                return Result<FinancialOperationResult>.Fail(treasuryResult.Errors, treasuryResult.Message);

            var entry = request.Lancamento with
            {
                Descricao = $"{documentNumber} - {request.Descricao.Trim()}",
                DocumentoReferencia = documentNumber,
                OrigemModulo = request.Modulo.Trim()
            };

            var accountingResult = await _accounting.SaveEntryAsync(entry, cancellationToken);
            if (accountingResult.IsFailure || accountingResult.Value <= 0)
                return Result<FinancialOperationResult>.Fail(accountingResult.Errors, accountingResult.Message);

            var posted = false;
            if (request.ContabilizarAutomaticamente)
            {
                var postResult = await _accounting.PostEntryAsync(accountingResult.Value, cancellationToken);
                if (postResult.IsFailure)
                    return Result<FinancialOperationResult>.Fail(postResult.Errors, postResult.Message);
                posted = true;
            }

            await _audit.WriteAsync(new AuditTrailRequest(
                request.EmpresaId,
                request.UtilizadorId,
                request.UtilizadorNome,
                request.Modulo,
                "OperacaoFinanceira",
                accountingResult.Value,
                posted ? "ProcessarEContabilizar" : "Processar",
                documentNumber), cancellationToken);

            var result = new FinancialOperationResult(documentNumber, accountingResult.Value, posted);
            await _events.PublishAsync(new FinancialOperationProcessedEvent(result, request.Modulo), cancellationToken);
            return Result<FinancialOperationResult>.Ok(result, "Operação financeira processada com sucesso.");
        }
        catch (Exception ex)
        {
            return Result<FinancialOperationResult>.Fail(ex.Message, "Não foi possível processar a operação financeira.");
        }
    }

    private static IReadOnlyList<string> Validate(FinancialOperationRequest request)
    {
        var errors = new List<string>();
        if (request.EmpresaId <= 0) errors.Add("Empresa inválida.");
        if (request.UtilizadorId <= 0) errors.Add("Utilizador inválido.");
        if (string.IsNullOrWhiteSpace(request.Modulo)) errors.Add("Módulo obrigatório.");
        if (string.IsNullOrWhiteSpace(request.Prefixo)) errors.Add("Prefixo obrigatório.");
        if (string.IsNullOrWhiteSpace(request.Descricao)) errors.Add("Descrição obrigatória.");
        if (request.Movimento.EmpresaId != request.EmpresaId) errors.Add("A empresa do movimento não corresponde à operação.");
        if (request.Lancamento.EmpresaId != request.EmpresaId) errors.Add("A empresa do lançamento não corresponde à operação.");
        return errors;
    }
}

public sealed record FinancialOperationProcessedEvent(
    FinancialOperationResult Result,
    string Module) : IDomainEvent;
