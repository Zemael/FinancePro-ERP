# Base de Dados

## Padrão
- SQL Server.
- Entity Framework Core 8.
- DbContext no projeto `FinancePro.Data`.
- Projeto de arranque para comandos EF: `FinancePro.UI`.

## Baseline
A migration oficial consolidada é `InitialFinancePro`.

## Comandos

```powershell
dotnet ef migrations add NomeDaMigration `
  --project .\FinancePro.Data\FinancePro.Data.csproj `
  --startup-project .\FinancePro.UI\FinancePro.UI.csproj

dotnet ef database update `
  --project .\FinancePro.Data\FinancePro.Data.csproj `
  --startup-project .\FinancePro.UI\FinancePro.UI.csproj
```

Não editar migrations já aplicadas em ambientes com dados reais.
