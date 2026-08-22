# FinancePro v6.69.1 — Transações resilientes na faturação

Correção da incompatibilidade entre `SqlServerRetryingExecutionStrategy` e as transações iniciadas pelo módulo de Faturação.

Duplicação, emissão definitiva, recebimentos totais ou parciais e notas fiscais passam a executar as respetivas transações dentro da estratégia recuperável criada pelo contexto. As operações relacionadas continuam agrupadas atomicamente.
