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
foreach (var schemaName in new[] { "020_AccountingPeriodClosing.sql", "021_FiscalYearClosing.sql", "022_FiscalYearSettlement.sql", "023_AccountingConsolidation.sql", "024_AdvancedReceivablesPayables.sql", "025_AdvancedPurchasing.sql", "026_AdvancedCommercialRevenue.sql", "027_InventoryStock.sql" })
{
    var schemaPath = Path.Combine(root, "FinancePro.Data", "Scripts", "Schema", schemaName);
    if (!File.Exists(schemaPath)) continue;
    var sql = await File.ReadAllTextAsync(schemaPath);
    if (!string.IsNullOrWhiteSpace(sql)) await db.Database.ExecuteSqlRawAsync(sql);
}

if (Has(args, "--validate")) {
    Console.WriteLine($"OK: ligação à base {db.Database.GetDbConnection().Database} em {db.Database.GetDbConnection().DataSource}.");
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

    if (!await db.ExerciciosFinanceiros.AnyAsync(x => x.EmpresaId == company.Id && x.Ano == 2026))
        db.ExerciciosFinanceiros.Add(new ExercicioFinanceiro { EmpresaId=company.Id, Ano=2026, DataInicio=new DateTime(2026,1,1), DataFim=new DateTime(2026,12,31), Padrao=true, Encerrado=false });

    if (!await db.Utilizadores.AnyAsync(x => x.Email == adminEmail))
        db.Utilizadores.Add(new Utilizador { NomeCompleto=adminName, Email=adminEmail, PasswordHash=BCrypt.Net.BCrypt.HashPassword(adminPassword), PerfilId=adminProfile.Id, EmpresaId=company.Id });

    await db.SaveChangesAsync(); await tx.CommitAsync();
    Console.WriteLine("Bootstrap concluído. Dados existentes foram preservados; apenas registos em falta foram criados.");
} catch { await tx.RollbackAsync(); throw; }
