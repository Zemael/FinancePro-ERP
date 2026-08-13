# FinancePro v6.17.0 — Documentos Comerciais e Linhas de Produtos

## Implementado
- Linhas de produtos em pedidos/ordens de compra e propostas/faturas.
- Quantidade, preço unitário, desconto, IVA, subtotal e total por linha.
- Total do documento recalculado automaticamente pelas linhas.
- Receção de compra gera entradas de stock e atualiza custo médio ponderado.
- Faturação de venda valida disponibilidade e gera saídas de stock.
- Referência do documento gravada nos movimentos de stock.
- Edição de itens bloqueada nas fases finais do documento.
- Interface de Compras e Receitas com seleção do documento e grelha de itens.
- Schema idempotente 028_DocumentLines.sql.

## Base de dados
Esta versão acrescenta CompraItens e VendaItens. Após Initialize, crie/aplique a migration EF da v6.17.0 para sincronizar o ModelSnapshot.
