# FinancePro v6.56.0 — Impostos e Segurança Social na Folha Salarial

- Regras parametrizáveis para imposto salarial.
- Contribuições de Segurança Social do trabalhador e da entidade empregadora.
- Taxa percentual, valor fixo, limites da base e período de vigência.
- Aplicação automática das regras válidas durante a geração da folha.
- Detalhe individual do imposto, contribuição do trabalhador e encargo patronal.
- Nova tabela idempotente `HumanResourcesPayrollRules` e novos campos na folha salarial.

## Atualização obrigatória

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
