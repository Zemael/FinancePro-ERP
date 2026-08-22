# FinancePro v6.66.1 — Gestão de Faturas

O módulo **Faturação** passa a ter um fluxo documental próprio, mantendo **Receitas** como módulo independente para controlo financeiro das contas a receber.

## Fluxo

1. Criar a fatura proforma.
2. Adicionar produtos ou serviços, quantidades, preços, descontos e IVA.
3. Validar a proforma.
4. Converter a proforma em fatura definitiva.
5. Registar o recebimento total ou parcial e emitir o respetivo recibo.
6. Emitir notas de crédito ou débito quando necessário.
7. Imprimir a proforma ou qualquer documento fiscal emitido.

## Validação no Windows

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
