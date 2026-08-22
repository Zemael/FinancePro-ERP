# FinancePro v6.48.0 — Avaliação e Encerramento de Investimentos

- Avaliações intercalares, finais e pós-investimento.
- Registo de benefícios realizados, custos adicionais e valor residual.
- Medição percentual da realização dos objetivos.
- Classificação final, recomendação, estado e lições aprendidas.
- Apuramento automático do benefício líquido da avaliação.
- Encerramento do investimento diretamente pela interface.
- Nova tabela idempotente `InvestmentEvaluations`, aplicada pelo Bootstrap.

## Atualização

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
