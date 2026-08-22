# FinancePro v6.65.25 — Plano de Contas da Serralharia

## Estrutura registada

- 1 — Ativo: 9 contas analíticas.
- 2 — Passivo: 4 contas analíticas.
- 3 — Património Líquido: 2 contas analíticas.
- 4 — Receitas: 4 contas analíticas.
- 5 — Custos/Despesas: 9 contas analíticas.

O plano é carregado automaticamente nas estruturas `PlanoContas` e
`EnterpriseChartAccounts`. A operação é idempotente e preserva contas já
registadas com o mesmo código.

A conta **5.9 — Depreciação** fica disponível para lançamentos e orçamento.
