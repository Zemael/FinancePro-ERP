# FinancePro v6.67.0 — Gestão documental de faturação

Esta versão adapta ao FinancePro as melhores práticas observadas numa rotina madura de faturação, sem transportar regras fiscais específicas do Brasil.

## Incluído

- Estados documentais claros desde a proforma até ao recibo.
- Desconto geral, frete, outras despesas e observações.
- Recálculo automático do valor final quando as linhas são alteradas.
- Fase documental apresentada na tabela.
- Impressão completa com totalizadores em FCFA.
- Atualização incremental e idempotente da base através do script de gestão documental.

## Validação no Windows

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
