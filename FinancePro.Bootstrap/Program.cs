using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

static string Arg(string[] args, string name, string fallback = "") {
    var p = Array.FindIndex(args, x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
    return p >= 0 && p + 1 < args.Length ? args[p + 1] : fallback;
}
static bool Has(string[] args, string name) => args.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
var cfg = new ConfigurationBuilder().SetBasePath(Path.Combine(root, "FinancePro.UI")).AddJsonFile("appsettings.json", false).Build();
var cs = cfg.GetConnectionString("FinanceProDb") ?? throw new InvalidOperationException("Connection string FinanceProDb não configurada.");
var options = new DbContextOptionsBuilder<FinanceProDbContext>().UseSqlServer(cs).Options;
await using var db = new FinanceProDbContext(options);

if (!await db.Database.CanConnectAsync()) throw new InvalidOperationException("Não foi possível ligar à base FinancePro.");

// Aplica a evolução v6.6 de forma idempotente pelo próprio .NET.
foreach (var schemaName in new[] { "020_AccountingPeriodClosing.sql", "021_FiscalYearClosing.sql", "022_FiscalYearSettlement.sql", "023_AccountingConsolidation.sql", "024_AdvancedReceivablesPayables.sql", "025_AdvancedPurchasing.sql", "026_AdvancedCommercialRevenue.sql", "027_InventoryStock.sql", "028_DocumentLines.sql", "029_FiscalDocuments.sql", "030_AutomaticAccounting.sql", "031_AdvancedAssets.sql", "032_AnalyticAccounting.sql", "033_ProjectProfitability.sql", "034_ProjectOperations.sql", "035_Investments.sql", "036_InvestmentExecution.sql", "037_InvestmentRiskTargets.sql", "038_InvestmentFinancing.sql", "039_InvestmentFinancingPayments.sql", "040_InvestmentCashFlows.sql", "041_InvestmentEvaluations.sql", "042_HumanResourcesEmployees.sql", "043_HumanResourcesPayroll.sql", "044_HumanResourcesAdjustments.sql", "045_HumanResourcesAbsences.sql", "046_HumanResourcesAttendance.sql", "047_HumanResourcesLeaveBalances.sql", "048_HumanResourcesPayrollRules.sql", "049_HumanResourcesEmployeeLoans.sql", "050_UserProfilePhoto.sql", "051_BankRegistrationFields.sql", "052_SerrurerieChartOfAccounts.sql", "053_SerrurerieCostCenters.sql", "054_InvoiceManagement.sql", "055_CompanyLogo.sql", "056_ProductServices.sql" })
{
    var schemaPath = Path.Combine(root, "FinancePro.Data", "Scripts", "Schema", schemaName);
    if (!File.Exists(schemaPath)) continue;
    var sql = await File.ReadAllTextAsync(schemaPath);
    if (!string.IsNullOrWhiteSpace(sql)) await db.Database.ExecuteSqlRawAsync(sql);
}

if (Has(args, "--validate")) {
    var requiredModuleTables = new[] { "Projects", "WorkOrders", "Investments", "HumanResourcesEmployees", "HumanResourcesPayrollRuns", "HumanResourcesAbsences", "HumanResourcesAttendance", "HumanResourcesLeaveBalances", "HumanResourcesPayrollRules", "HumanResourcesEmployeeLoans" };
    var missingModuleTables = new List<string>();
    var validationConnection = db.Database.GetDbConnection();
    var closeValidationConnection = validationConnection.State != System.Data.ConnectionState.Open;
    if (closeValidationConnection) await validationConnection.OpenAsync();
    try {
        foreach (var table in requiredModuleTables) {
            await using var command = validationConnection.CreateCommand();
            command.CommandText = $"SELECT CASE WHEN OBJECT_ID(N'dbo.{table}',N'U') IS NULL THEN 0 ELSE 1 END";
            if (Convert.ToInt32(await command.ExecuteScalarAsync()) == 0) missingModuleTables.Add(table);
        }
        await using var photoCommand = validationConnection.CreateCommand();
        photoCommand.CommandText = "SELECT CASE WHEN COL_LENGTH(N'dbo.Utilizadores', N'FotoPerfil') IS NULL THEN 0 ELSE 1 END";
        if (Convert.ToInt32(await photoCommand.ExecuteScalarAsync()) == 0)
            throw new InvalidOperationException("Estrutura incompleta. Coluna Utilizadores.FotoPerfil em falta.");
        await using var bankFieldsCommand = validationConnection.CreateCommand();
        bankFieldsCommand.CommandText = "SELECT CASE WHEN COL_LENGTH(N'dbo.Bancos', N'Sigla') IS NOT NULL AND COL_LENGTH(N'dbo.Bancos', N'Endereco') IS NOT NULL AND COL_LENGTH(N'dbo.Bancos', N'Contacto') IS NOT NULL THEN 1 ELSE 0 END";
        if (Convert.ToInt32(await bankFieldsCommand.ExecuteScalarAsync()) == 0)
            throw new InvalidOperationException("Estrutura incompleta. Campos adicionais do cadastro de Bancos em falta.");
        await using var invoiceFieldsCommand = validationConnection.CreateCommand();
        invoiceFieldsCommand.CommandText = "SELECT CASE WHEN COL_LENGTH(N'dbo.ContasReceber',N'DescontoGeral') IS NOT NULL AND COL_LENGTH(N'dbo.ContasReceber',N'Frete') IS NOT NULL AND COL_LENGTH(N'dbo.ContasReceber',N'OutrasDespesas') IS NOT NULL AND COL_LENGTH(N'dbo.ContasReceber',N'Observacoes') IS NOT NULL THEN 1 ELSE 0 END";
        if (Convert.ToInt32(await invoiceFieldsCommand.ExecuteScalarAsync()) == 0)
            throw new InvalidOperationException("Estrutura incompleta. Campos de gestão documental da faturação em falta.");
        await using var companyLogoCommand = validationConnection.CreateCommand();
        companyLogoCommand.CommandText = "SELECT CASE WHEN COL_LENGTH(N'dbo.Empresas',N'Logotipo') IS NOT NULL THEN 1 ELSE 0 END";
        if (Convert.ToInt32(await companyLogoCommand.ExecuteScalarAsync()) == 0)
            throw new InvalidOperationException("Estrutura incompleta. Campo Empresas.Logotipo em falta.");
        await using var catalogCommand = validationConnection.CreateCommand();
        catalogCommand.CommandText = "SELECT CASE WHEN COL_LENGTH(N'dbo.Produtos',N'PrecoVenda') IS NOT NULL AND COL_LENGTH(N'dbo.Produtos',N'ControlaStock') IS NOT NULL THEN 1 ELSE 0 END";
        if (Convert.ToInt32(await catalogCommand.ExecuteScalarAsync()) == 0)
            throw new InvalidOperationException("Estrutura incompleta. Campos comerciais de Produtos e Serviços em falta.");
        await using var chartCommand = validationConnection.CreateCommand();
        chartCommand.CommandText = "SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.PlanoContas WHERE Codigo=N'5.9' AND Nome=N'Depreciação') THEN 1 ELSE 0 END";
        if (Convert.ToInt32(await chartCommand.ExecuteScalarAsync()) == 0)
            throw new InvalidOperationException("Plano de contas da serralharia ainda não foi registado.");
        await using var costCenterCommand = validationConnection.CreateCommand();
        costCenterCommand.CommandText = "SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.CostCenters WHERE Code=N'CC14' AND Name=N'Segurança do trabalho') THEN 1 ELSE 0 END";
        if (Convert.ToInt32(await costCenterCommand.ExecuteScalarAsync()) == 0)
            throw new InvalidOperationException("Centros de custo da serralharia ainda não foram registados.");
    } finally {
        if (closeValidationConnection) await validationConnection.CloseAsync();
    }
    if (missingModuleTables.Count > 0)
        throw new InvalidOperationException($"Estrutura incompleta. Tabelas em falta: {string.Join(", ", missingModuleTables)}.");
    Console.WriteLine($"OK: ligação à base {db.Database.GetDbConnection().Database} em {db.Database.GetDbConnection().DataSource}.");
    Console.WriteLine("OK: módulos Projetos, Investimentos e RH Financeiro disponíveis.");
    Console.WriteLine($"Empresas={await db.Empresas.CountAsync()}; Utilizadores={await db.Utilizadores.CountAsync()}; Moedas={await db.Moedas.CountAsync()}; Exercícios={await db.ExerciciosFinanceiros.CountAsync()}");
    return;
}

var companyName = Arg(args, "--company", "FinancePro");
var adminName = Arg(args, "--admin-name", "Administrador FinancePro");
var adminEmail = Arg(args, "--admin-email", "admin@financepro.local").Trim().ToLowerInvariant();
var adminPassword = Arg(args, "--admin-password");
if (string.IsNullOrWhiteSpace(adminPassword) || adminPassword.Length < 8)
    throw new InvalidOperationException("Informe --admin-password com pelo menos 8 caracteres.");

await using var tx = await db.Database.BeginTransactionAsync();
try {
    var currency = await db.Moedas.FirstOrDefaultAsync(x => x.CodigoIso == "XOF");
    if (currency is null) { currency = new Moeda { CodigoIso="XOF", Nome="Franco CFA BCEAO", Simbolo="FCFA", CasasDecimais=0 }; db.Moedas.Add(currency); }

    var adminProfile = await db.Perfis.FirstOrDefaultAsync(x => x.Nome == "Administrador");
    if (adminProfile is null) { adminProfile = new Perfil { Nome="Administrador", Descricao="Acesso total ao sistema" }; db.Perfis.Add(adminProfile); }
    foreach (var (n,d) in new[]{("Gestor","Acesso aos módulos financeiros e relatórios"),("Operador","Acesso limitado a lançamentos do dia a dia")})
        if (!await db.Perfis.AnyAsync(x => x.Nome == n)) db.Perfis.Add(new Perfil { Nome=n, Descricao=d });
    await db.SaveChangesAsync();

    var company = await db.Empresas.OrderBy(x=>x.Id).FirstOrDefaultAsync();
    if (company is null) { company = new Empresa { Nome=companyName, Moeda="FCFA" }; db.Empresas.Add(company); await db.SaveChangesAsync(); }

    var chartSeedPath = Path.Combine(root, "FinancePro.Data", "Scripts", "Schema", "052_SerrurerieChartOfAccounts.sql");
    if (File.Exists(chartSeedPath))
    {
        var chartSeedSql = await File.ReadAllTextAsync(chartSeedPath);
        if (!string.IsNullOrWhiteSpace(chartSeedSql)) await db.Database.ExecuteSqlRawAsync(chartSeedSql);
    }

    var costCenterSeedPath = Path.Combine(root, "FinancePro.Data", "Scripts", "Schema", "053_SerrurerieCostCenters.sql");
    if (File.Exists(costCenterSeedPath))
    {
        var costCenterSeedSql = await File.ReadAllTextAsync(costCenterSeedPath);
        if (!string.IsNullOrWhiteSpace(costCenterSeedSql)) await db.Database.ExecuteSqlRawAsync(costCenterSeedSql);
    }

    if (!await db.ExerciciosFinanceiros.AnyAsync(x => x.EmpresaId == company.Id && x.Ano == 2026))
        db.ExerciciosFinanceiros.Add(new ExercicioFinanceiro { EmpresaId=company.Id, Ano=2026, DataInicio=new DateTime(2026,1,1), DataFim=new DateTime(2026,12,31), Padrao=true, Encerrado=false });

    if (!await db.Utilizadores.AnyAsync(x => x.Email == adminEmail))
        db.Utilizadores.Add(new Utilizador { NomeCompleto=adminName, Email=adminEmail, PasswordHash=BCrypt.Net.BCrypt.HashPassword(adminPassword), PerfilId=adminProfile.Id, EmpresaId=company.Id });

    await db.SaveChangesAsync(); await tx.CommitAsync();
    Console.WriteLine("Bootstrap concluído. Dados existentes foram preservados; apenas registos em falta foram criados.");
} catch { await tx.RollbackAsync(); throw; }
