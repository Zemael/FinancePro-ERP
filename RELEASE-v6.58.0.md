# FinancePro v6.58.0 — Relatórios Consolidados do RH Financeiro

- Resumo automático de colaboradores e custos por departamento.
- Total de salários-base, subsídios, descontos e líquido mensal.
- Exportação CSV do cadastro de colaboradores.
- Exportação CSV da folha salarial selecionada, com impostos, contribuições e empréstimos.
- Exportação CSV do custo remuneratório por departamento.
- Conclusão funcional do módulo RH Financeiro.
- Não altera a base de dados.

## Atualização

```powershell
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
