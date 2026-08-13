# FinancePro v6.16.0 — Gestão de Stock, Produtos e Movimentações

- Cadastro de produtos, categorias, unidades, localização e stock mínimo.
- Entradas, saídas e ajustes com validação de stock disponível.
- Custo médio ponderado nas entradas e valorização do stock.
- Histórico dos últimos movimentos e documento de referência.
- Alertas de stock mínimo e valor consolidado do inventário.
- Schema idempotente 027_InventoryStock.sql.
- Módulo Stocks integrado ao menu principal.

Nota: a integração automática por item de Compra/Venda fica preparada por DocumentoReferencia; a baseline atual não possui linhas/itens de compra/venda, portanto não foi criada baixa/entrada automática sem uma relação de produto que pudesse garantir integridade.
