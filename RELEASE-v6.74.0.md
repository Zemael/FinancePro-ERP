# FinancePro v6.74.0 — Produtos e Serviços

O catálogo comercial passa a distinguir produtos com controlo de stock de serviços sem inventário.

Cada item pode ter um preço de venda sugerido. Ao selecionar o produto ou serviço numa linha da fatura, o preço é preenchido automaticamente e continua editável.

Produtos mantêm a validação de disponibilidade e geram saída automática de stock na fatura definitiva. Serviços podem ser faturados normalmente, sem exigir saldo nem criar movimentos de inventário.

O Bootstrap aplica os campos `PrecoVenda` e `ControlaStock` de forma idempotente através do script `056_ProductServices.sql`.
