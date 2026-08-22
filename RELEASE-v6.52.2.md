# FinancePro v6.52.2 — Correção do Recurso Visual Card

- Corrigida a exceção `Cannot find resource named 'Card'` ao abrir Investimentos.
- Adicionado alias global `Card`, baseado no estilo oficial `FP.Card`.
- A correção aplica-se simultaneamente às vistas Projetos, Investimentos e RH Financeiro.
- Não altera a base de dados e não requer Bootstrap adicional após a v6.52.1.

## Atualização

```powershell
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
