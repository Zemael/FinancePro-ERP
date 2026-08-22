# FinancePro v6.46.0 — Amortização e Prestações de Investimentos

- Geração automática do plano de amortização pelo sistema francês de prestações constantes.
- Separação de capital, juros e total devido em cada vencimento.
- Registo de pagamentos totais ou parciais, data, referência e notas.
- Identificação automática de prestações vencidas como atrasadas.
- Atualização do saldo devedor do financiamento após cada pagamento.
- Nova tabela idempotente `InvestmentFinancingPayments`, aplicada pelo Bootstrap.

## Atualização

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
