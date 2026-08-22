# FinancePro v6.68.0 — Faturação operacional

## Novo fluxo

1. Criar rascunho da fatura proforma.
2. O FinancePro seleciona automaticamente o novo documento.
3. Adicionar produtos ou serviços, quantidades, preços, descontos e IVA.
4. O total é calculado automaticamente.
5. Validar a proforma apenas quando possuir linhas e valor positivo.
6. Converter em fatura definitiva.
7. Registar recebimentos e emitir recibos.

O campo de valor manual foi mantido apenas no módulo Receitas. Na Faturação, o total é sempre derivado das linhas e dos totalizadores do documento.

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
