# FinancePro v6.19.1 — Configuração das Regras Contabilísticas

- Nova interface visual em Contabilidade > Regras Automáticas.
- Configuração por empresa das contas de débito/crédito para VENDA_FATURA, VENDA_RECEBIMENTO, COMPRA_FATURA e COMPRA_PAGAMENTO.
- Ativação/desativação de regras e validação contra contas iguais ou não selecionadas.
- Persistência idempotente sobre dbo.AutomaticAccountingRules, sem nova migration ou alteração de schema.
