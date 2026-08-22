# FinancePro v6.50.0 — Processamento da Folha Salarial

- Criação da folha salarial por ano e mês.
- Geração automática para todos os colaboradores ativos.
- Congelamento dos valores de salário-base, subsídios e descontos no período.
- Apuramento automático dos totais bruto, descontos e líquido.
- Detalhe individual por colaborador.
- Aprovação formal da folha salarial.
- Novas tabelas idempotentes `HumanResourcesPayrollRuns` e `HumanResourcesPayrollLines`.

## Atualização

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
