using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FinancePro.Data.Context;

/// <summary>
/// Usada só pelas ferramentas do EF Core (Add-Migration, Update-Database)
/// para construir o DbContext em tempo de design, já que a app WPF não
/// tem o padrão Program.cs que essas ferramentas detetam automaticamente.
///
/// Lê a MESMA connection string do FinancePro.UI/appsettings.json que a
/// app usa em execução (com um valor de recurso caso o ficheiro não seja
/// encontrado), para nunca haver desfasamento entre a base de dados onde
/// as migrações são aplicadas e a base de dados a que a app liga.
/// </summary>
public class FinanceProDbContextFactory : IDesignTimeDbContextFactory<FinanceProDbContext>
{
    private const string ConnectionStringRecurso =
        "Server=.\\SQLEXPRESS;Database=FinanceProDb;Trusted_Connection=True;TrustServerCertificate=True;";

    public FinanceProDbContext CreateDbContext(string[] args)
    {
        var connectionString = ObterConnectionStringDoAppsettings() ?? ConnectionStringRecurso;

        var optionsBuilder = new DbContextOptionsBuilder<FinanceProDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return new FinanceProDbContext(optionsBuilder.Options);
    }

    private static string? ObterConnectionStringDoAppsettings()
    {
        // O comando `dotnet ef` corre com a pasta do projeto FinancePro.Data
        // como diretório atual; o appsettings.json real vive em FinancePro.UI.
        var caminhoAppsettings = Path.Combine(Directory.GetCurrentDirectory(), "..", "FinancePro.UI", "appsettings.json");

        if (!File.Exists(caminhoAppsettings))
        {
            return null;
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(caminhoAppsettings, optional: true)
            .Build();

        return configuration.GetConnectionString("FinanceProDb");
    }
}
