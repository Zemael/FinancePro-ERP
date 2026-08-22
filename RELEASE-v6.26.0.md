# FinancePro v6.26.0 — Integração Operacional de Projetos

- ProjectId em Compras, Contas a Receber, Contas a Pagar e Movimentos de Stock.
- WorkOrderId em movimentos de stock.
- Receita real por projeto derivada das vendas faturadas.
- Custo real consolidado por projeto: contas a pagar, compras ainda não faturadas e consumo de stock.
- Resultado e margem calculados automaticamente no painel de projetos.
- Schema idempotente 034_ProjectOperations.sql; não criar migration manual.
