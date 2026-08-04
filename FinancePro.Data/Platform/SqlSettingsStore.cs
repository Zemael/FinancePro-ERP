using System.Data;
using FinancePro.Data.Context;
using FinancePro.Platform.Settings;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Platform;

public sealed class SqlSettingsStore : ISettingsStore
{
    private readonly FinanceProDbContext _dbContext;
    public SqlSettingsStore(FinanceProDbContext dbContext) => _dbContext = dbContext;

    public async Task<SystemSetting?> FindAsync(int companyId, string category, string key, CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT TOP (1) Id, CompanyId, Category, [Key], [Value], DataType, Description, IsEditable, UpdatedAt, UpdatedBy FROM dbo.SystemSettings WHERE CompanyId=@companyId AND Category=@category AND [Key]=@key";
            Add(command, "@companyId", companyId);
            Add(command, "@category", category);
            Add(command, "@key", key);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        finally { if (close) await connection.CloseAsync(); }
    }

    public async Task<IReadOnlyList<SystemSetting>> ListCategoryAsync(int companyId, string category, CancellationToken cancellationToken = default)
    {
        var result = new List<SystemSetting>();
        var connection = _dbContext.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, CompanyId, Category, [Key], [Value], DataType, Description, IsEditable, UpdatedAt, UpdatedBy FROM dbo.SystemSettings WHERE CompanyId=@companyId AND Category=@category ORDER BY [Key]";
            Add(command, "@companyId", companyId);
            Add(command, "@category", category);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader));
            return result;
        }
        finally { if (close) await connection.CloseAsync(); }
    }

    public async Task UpsertAsync(int companyId, string category, string key, string? value, string dataType, string? description, int? updatedBy, CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"MERGE dbo.SystemSettings AS target
USING (SELECT @companyId CompanyId, @category Category, @key [Key]) AS source
ON target.CompanyId=source.CompanyId AND target.Category=source.Category AND target.[Key]=source.[Key]
WHEN MATCHED THEN UPDATE SET [Value]=@value, DataType=@dataType, Description=COALESCE(@description, target.Description), UpdatedAt=SYSUTCDATETIME(), UpdatedBy=@updatedBy
WHEN NOT MATCHED THEN INSERT (CompanyId, Category, [Key], [Value], DataType, Description, IsEditable, UpdatedAt, UpdatedBy)
VALUES (@companyId, @category, @key, @value, @dataType, @description, 1, SYSUTCDATETIME(), @updatedBy);";
            Add(command, "@companyId", companyId); Add(command, "@category", category); Add(command, "@key", key);
            Add(command, "@value", value); Add(command, "@dataType", dataType); Add(command, "@description", description); Add(command, "@updatedBy", updatedBy);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally { if (close) await connection.CloseAsync(); }
    }

    private static void Add(System.Data.Common.DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter(); parameter.ParameterName = name; parameter.Value = value ?? DBNull.Value; command.Parameters.Add(parameter);
    }

    private static SystemSetting Map(System.Data.Common.DbDataReader reader) => new(
        reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4),
        reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6), reader.GetBoolean(7), reader.GetDateTime(8), reader.IsDBNull(9) ? null : reader.GetInt32(9));
}
