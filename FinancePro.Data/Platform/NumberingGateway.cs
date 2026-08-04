using System.Data;
using System.Data.Common;
using FinancePro.Application.Platform;
using FinancePro.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Platform;

public sealed class NumberingGateway : INumberingGateway
{
    private readonly FinanceProDbContext _db;
    public NumberingGateway(FinanceProDbContext db) => _db = db;

    public async Task<string> GetNextAsync(DocumentNumberRequest request, CancellationToken cancellationToken = default)
    {
        var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var year = request.ReiniciarAnualmente ? request.DataDocumento.Year : 0;
            var current = await ReadCurrentAsync(connection, transaction, request, year, cancellationToken);
            var next = current + 1;
            await UpsertAsync(connection, transaction, request, year, next, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var sequence = next.ToString().PadLeft(request.Digitos, '0');
            return request.ReiniciarAnualmente
                ? $"{request.Prefixo}-{request.DataDocumento.Year}-{sequence}"
                : $"{request.Prefixo}-{sequence}";
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<int> ReadCurrentAsync(DbConnection connection, DbTransaction transaction, DocumentNumberRequest request, int year, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT UltimoNumero FROM SequenciasDocumentos WITH (UPDLOCK, HOLDLOCK) WHERE EmpresaId=@empresa AND Modulo=@modulo AND Ano=@ano";
        Add(command, "@empresa", request.EmpresaId);
        Add(command, "@modulo", request.Modulo);
        Add(command, "@ano", year);
        var value = await command.ExecuteScalarAsync(ct);
        return value is null or DBNull ? 0 : Convert.ToInt32(value);
    }

    private static async Task UpsertAsync(DbConnection connection, DbTransaction transaction, DocumentNumberRequest request, int year, int next, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
IF EXISTS (SELECT 1 FROM SequenciasDocumentos WHERE EmpresaId=@empresa AND Modulo=@modulo AND Ano=@ano)
    UPDATE SequenciasDocumentos SET Prefixo=@prefixo, UltimoNumero=@numero, Digitos=@digitos, ReiniciarAnualmente=@reiniciar, DataAtualizacao=SYSUTCDATETIME()
    WHERE EmpresaId=@empresa AND Modulo=@modulo AND Ano=@ano;
ELSE
    INSERT INTO SequenciasDocumentos(EmpresaId, Modulo, Prefixo, Ano, UltimoNumero, Digitos, ReiniciarAnualmente, Ativo, DataCriacao)
    VALUES(@empresa, @modulo, @prefixo, @ano, @numero, @digitos, @reiniciar, 1, SYSUTCDATETIME());";
        Add(command, "@empresa", request.EmpresaId);
        Add(command, "@modulo", request.Modulo);
        Add(command, "@prefixo", request.Prefixo);
        Add(command, "@ano", year);
        Add(command, "@numero", next);
        Add(command, "@digitos", request.Digitos);
        Add(command, "@reiniciar", request.ReiniciarAnualmente);
        await command.ExecuteNonQueryAsync(ct);
    }

    private static void Add(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
