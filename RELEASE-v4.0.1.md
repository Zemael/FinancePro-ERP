# FinancePro v4.0.1 — Dashboard Design System: Bancos, Caixa e Tesouraria

## Entregue

- Bancos e contas bancárias alinhados ao visual do Dashboard.
- Caixa com indicadores operacionais e cabeçalho padronizado.
- Tesouraria com KPIs, atualização manual e painel de registo lateral.
- Novos indicadores calculados nos respetivos ViewModels.
- Nenhuma alteração ao modelo de dados e nenhuma migration necessária.

## Validação recomendada

```powershell
dotnet clean .\FinancePro.sln
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
