using FinancePro.Application.Platform;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;

namespace FinancePro.Data.Platform;

public sealed class AuditTrailGateway : IAuditTrailGateway
{
    private readonly FinanceProDbContext _db;
    public AuditTrailGateway(FinanceProDbContext db) => _db = db;

    public async Task WriteAsync(AuditTrailRequest request, CancellationToken cancellationToken = default)
    {
        _db.LogsAuditoria.Add(new LogAuditoria
        {
            Data = DateTime.UtcNow,
            EmpresaId = request.EmpresaId,
            UtilizadorId = request.UtilizadorId,
            UtilizadorNome = request.UtilizadorNome,
            Entidade = request.Entidade,
            RegistoId = request.RegistoId,
            Acao = request.Operacao,
            Detalhe = string.IsNullOrWhiteSpace(request.Detalhe)
                ? $"Módulo: {request.Modulo}"
                : $"Módulo: {request.Modulo}; {request.Detalhe}"
        });
        await _db.SaveChangesAsync(cancellationToken);
    }
}
