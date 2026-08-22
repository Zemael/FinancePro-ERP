# FinancePro v6.53.0 — Férias, Licenças e Faltas

- Registo de férias, licenças e faltas por colaborador.
- Período, número de dias, motivo, notas e indicação de ausência remunerada.
- Fluxo de decisão com estados pendente, aprovada e rejeitada.
- Desconto configurável para ausências não remuneradas.
- Inclusão automática dos descontos aprovados na folha do mês de início da ausência.
- Nova tabela idempotente `HumanResourcesAbsences`, aplicada pelo Bootstrap.

## Atualização obrigatória

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
