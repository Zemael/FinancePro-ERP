# FinancePro v6.67.1 — Correção da estrutura de faturação

Corrige a incompatibilidade entre a aplicação v6.67.0 e bases existentes que ainda não possuíam as colunas `DescontoGeral`, `Frete`, `OutrasDespesas` e `Observacoes`.

O módulo confirma e cria automaticamente os campos em falta antes da primeira consulta ou gravação. O Bootstrap também aplica e valida explicitamente o script `054_InvoiceManagement.sql`.

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
