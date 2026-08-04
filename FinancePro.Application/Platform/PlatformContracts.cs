namespace FinancePro.Application.Platform;

public interface INumberingGateway
{
    Task<string> GetNextAsync(DocumentNumberRequest request, CancellationToken cancellationToken = default);
}

public interface IAuditTrailGateway
{
    Task WriteAsync(AuditTrailRequest request, CancellationToken cancellationToken = default);
}

public interface INumberingService
{
    Task<string> GetNextAsync(DocumentNumberRequest request, CancellationToken cancellationToken = default);
}

public interface IAuditTrailService
{
    Task WriteAsync(AuditTrailRequest request, CancellationToken cancellationToken = default);
}

public interface IFinancialEngine
{
    Task<Common.Results.Result<FinancialOperationResult>> ProcessAsync(
        FinancialOperationRequest request,
        CancellationToken cancellationToken = default);
}

public interface IDomainEvent { }

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}

public interface IDomainEventDispatcher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}
