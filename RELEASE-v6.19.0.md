# FinancePro v6.19.0 — Contabilidade Integrada

## Implementado
- Motor de contabilização automática configurável por empresa e evento.
- Eventos: VENDA_FATURA, VENDA_RECEBIMENTO, COMPRA_FATURA e COMPRA_PAGAMENTO.
- Lançamentos gerados já contabilizados, balanceados em débito/crédito.
- Rastreabilidade entre evento operacional e AccountingEntry.
- Idempotência por empresa + evento + origem, evitando duplicação.
- Pagamentos e recebimentos parciais usam o movimento de tesouraria como origem única.
- Sem regras configuradas, a operação financeira continua normalmente e nenhum lançamento incorreto é criado.

## Configuração
A tabela dbo.AutomaticAccountingRules associa cada EventCode a DebitAccountId e CreditAccountId do plano de contas da empresa. Apenas regras ativas geram lançamentos.
