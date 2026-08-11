# FinancePro v6.9.0 — Relatórios Financeiros e Contabilísticos

## Entrega

A Central de Relatórios passa a reunir relatórios operacionais e demonstrações contabilísticas num único fluxo por empresa e período.

### Novos relatórios contabilísticos

- Diário Contabilístico
- Razão Geral com saldo acumulado por conta
- Balancete com saldos iniciais, movimentos e saldos finais
- DRE com comparativo automático do período imediatamente anterior
- Balanço Patrimonial com comparação à posição anterior
- Fluxo de Caixa por atividades operacionais, investimento e financiamento

### Exportação e impressão

- CSV completo em UTF-8
- HTML pronto para impressão
- abertura do relatório no navegador para impressão ou "Guardar como PDF"

### Integração

Os relatórios utilizam as tabelas contabilísticas existentes (`AccountingEntries`, `AccountingEntryLines` e `EnterpriseChartAccounts`). Não é necessária nova migration ou novo script SQL nesta versão.

### Validação

Execute:

```powershell
.\Validate-FinancePro.ps1
.\Build-FinancePro.ps1
```
