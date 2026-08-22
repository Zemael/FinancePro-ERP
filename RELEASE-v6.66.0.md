# FinancePro v6.66.0 — Módulo de Faturação

## Alterações

- O item inativo **Demonstrações Financeiras** foi removido do menu e substituído por **Faturação**.
- O novo módulo possui código de permissão próprio: `Faturacao`.
- A faturação utiliza o fluxo comercial já integrado no FinancePro:
  - criação e aprovação de propostas;
  - inclusão e remoção de linhas;
  - cálculo de preço, desconto e IVA;
  - emissão numerada de faturas;
  - baixa automática de stock;
  - criação de recibos por recebimento total ou parcial;
  - emissão de notas de crédito e notas de débito;
  - integração com Tesouraria e Contabilidade.
- O módulo **Receitas** foi preservado para acompanhamento das contas a receber.

## Validação no Windows

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
