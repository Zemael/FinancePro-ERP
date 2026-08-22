# FinancePro v6.55.0 — Saldos Anuais de Férias

- Definição do direito anual de férias por colaborador.
- Registo de dias transitados do exercício anterior.
- Apuramento automático dos dias utilizados através das férias aprovadas.
- Cálculo imediato do saldo disponível.
- Histórico anual editável e arquivável.
- Nova tabela idempotente `HumanResourcesLeaveBalances`, aplicada pelo Bootstrap.

## Atualização obrigatória

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
