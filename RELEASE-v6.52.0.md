# FinancePro v6.52.0 — Componentes Remuneratórios

- Cadastro de abonos e descontos por colaborador.
- Categorias para subsídios, prémios, horas extraordinárias, impostos, Segurança Social, adiantamentos e faltas.
- Componentes recorrentes ou temporários, com período de vigência.
- Inclusão automática dos componentes válidos na geração da folha salarial.
- Histórico editável e arquivável por colaborador.
- Nova tabela idempotente `HumanResourcesEmployeeAdjustments`, aplicada pelo Bootstrap.

## Atualização

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
