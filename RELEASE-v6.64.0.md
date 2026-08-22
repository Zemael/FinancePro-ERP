# FinancePro v6.64.0 — Acabamento Funcional e Visual de Stocks

- Pesquisa instantânea por código, nome, categoria e localização.
- Filtro específico para produtos abaixo do stock mínimo.
- Edição completa do cadastro de produtos.
- Arquivo lógico protegido: apenas produtos com stock igual a zero.
- Exportação CSV do inventário filtrado e dos movimentos.
- Observações nos movimentos manuais de stock.
- Indicadores de produtos ativos, valor do inventário, alertas e ruturas.
- Interface reorganizada e alinhada ao framework visual do FinancePro.
- Nenhuma alteração à base de dados.

## Validação

```powershell
powershell -ExecutionPolicy Bypass -File .\Gerar-Release.ps1
```
