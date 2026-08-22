# FinancePro v6.51.0 — Pagamento da Folha Salarial

- Registo individual de pagamentos por colaborador.
- Data, referência bancária e notas do pagamento.
- Pagamento integral da folha salarial em lote.
- Estados automáticos: aprovada, parcialmente paga e paga.
- Histórico de pagamento apresentado no detalhe da folha.
- Reutiliza as tabelas da v6.50.0; não requer novo script de base de dados.

## Atualização

```powershell
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
