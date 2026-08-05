using System.Data;
using System.Data.Common;
using FinancePro.Data.Context;
using FinancePro.Platform.Administration;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.AdministrationMasterData;

public sealed class SqlAdministrationMasterDataStore : IAdministrationMasterDataStore
{
    private readonly FinanceProDbContext _dbContext;
    public SqlAdministrationMasterDataStore(FinanceProDbContext dbContext) => _dbContext = dbContext;

    public Task<IReadOnlyList<CostCenter>> ListCostCentersAsync(int companyId, CancellationToken cancellationToken = default) =>
        QueryAsync("SELECT Id, CompanyId, Code, Name, Active FROM dbo.CostCenters WHERE CompanyId=@companyId ORDER BY Code", companyId,
            r => new CostCenter(r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.GetString(3), r.GetBoolean(4)), cancellationToken);

    public Task<IReadOnlyList<TaxRate>> ListTaxRatesAsync(int companyId, CancellationToken cancellationToken = default) =>
        QueryAsync("SELECT Id, CompanyId, Code, Name, Rate, Active FROM dbo.TaxRates WHERE CompanyId=@companyId ORDER BY Code", companyId,
            r => new TaxRate(r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.GetString(3), r.GetDecimal(4), r.GetBoolean(5)), cancellationToken);

    public Task SaveCostCenterAsync(SaveCostCenterRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(@"
IF @id IS NULL
 INSERT INTO dbo.CostCenters (CompanyId, Code, Name, Active, CreatedAt, UpdatedAt) VALUES (@companyId,@code,@name,@active,SYSUTCDATETIME(),SYSUTCDATETIME());
ELSE
 UPDATE dbo.CostCenters SET Code=@code, Name=@name, Active=@active, UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId;",
        request.CompanyId, request.Id, request.Code, request.Name, null, request.Active, cancellationToken);

    public Task SaveTaxRateAsync(SaveTaxRateRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(@"
IF @id IS NULL
 INSERT INTO dbo.TaxRates (CompanyId, Code, Name, Rate, Active, CreatedAt, UpdatedAt) VALUES (@companyId,@code,@name,@rate,@active,SYSUTCDATETIME(),SYSUTCDATETIME());
ELSE
 UPDATE dbo.TaxRates SET Code=@code, Name=@name, Rate=@rate, Active=@active, UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId;",
        request.CompanyId, request.Id, request.Code, request.Name, request.Rate, request.Active, cancellationToken);

    public Task SetCostCenterActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default) =>
        ExecuteAsync("UPDATE dbo.CostCenters SET Active=@active, UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId", companyId, id, "", "", null, active, cancellationToken);

    public Task SetTaxRateActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default) =>
        ExecuteAsync("UPDATE dbo.TaxRates SET Active=@active, UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId", companyId, id, "", "", null, active, cancellationToken);

    private async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, int companyId, Func<DbDataReader,T> map, CancellationToken ct)
    {
        var result = new List<T>(); var connection = _dbContext.Database.GetDbConnection(); var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(ct);
        try { await using var command = connection.CreateCommand(); command.CommandText = sql; Add(command,"@companyId",companyId); await using var reader = await command.ExecuteReaderAsync(ct); while (await reader.ReadAsync(ct)) result.Add(map(reader)); return result; }
        finally { if (close) await connection.CloseAsync(); }
    }

    private async Task ExecuteAsync(string sql, int companyId, int? id, string code, string name, decimal? rate, bool active, CancellationToken ct)
    {
        var connection = _dbContext.Database.GetDbConnection(); var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(ct);
        try { await using var command = connection.CreateCommand(); command.CommandText = sql; Add(command,"@companyId",companyId); Add(command,"@id",id); Add(command,"@code",code); Add(command,"@name",name); Add(command,"@rate",rate); Add(command,"@active",active); await command.ExecuteNonQueryAsync(ct); }
        finally { if (close) await connection.CloseAsync(); }
    }

    private static void Add(DbCommand command, string name, object? value) { var p=command.CreateParameter(); p.ParameterName=name; p.Value=value ?? DBNull.Value; command.Parameters.Add(p); }
}
