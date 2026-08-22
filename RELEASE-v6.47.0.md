# FinancePro v6.47.0 — Fluxos de Caixa e Viabilidade de Investimentos

- Registo de entradas e saídas previstas ou realizadas por investimento.
- Apuramento automático do fluxo líquido acumulado.
- Cálculo do Valor Presente Líquido (VPL) com taxa de desconto configurável.
- Identificação automática da data de payback.
- Histórico com data, descrição, referência e notas.
- Nova tabela idempotente `InvestmentCashFlows`, aplicada pelo Bootstrap.

## Atualização

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
