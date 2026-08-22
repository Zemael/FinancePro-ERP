# FinancePro v6.54.0 — Assiduidade e Horas Extraordinárias

- Registo diário de entrada e saída por colaborador.
- Horas normais, horas extraordinárias e respetivo valor remuneratório.
- Fluxo de aprovação ou rejeição antes do processamento salarial.
- Inclusão automática das horas extraordinárias aprovadas na folha do mês.
- Edição reinicia o registo para o estado pendente.
- Nova tabela idempotente `HumanResourcesAttendance`, aplicada pelo Bootstrap.

## Atualização obrigatória

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
