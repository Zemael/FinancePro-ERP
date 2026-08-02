# FinancePro ERP v2.0.0 — Foundation

Esta release cria a camada Application e os contratos comuns, sem alterar o modelo da base de dados.

## Teste

```powershell
dotnet clean .\FinancePro.sln
dotnet build .\FinancePro.sln
dotnet ef migrations has-pending-model-changes `
  --project .\FinancePro.Data\FinancePro.Data.csproj `
  --startup-project .\FinancePro.UI\FinancePro.UI.csproj
```

Resultado esperado: build com sucesso e nenhuma alteração pendente no modelo.
