# FinancePro ERP v1.5 — Transferências

## Entrega
- Transferência entre Caixa e Conta Bancária.
- Transferência entre duas contas bancárias.
- Transferência entre dois caixas.
- Geração automática de dois movimentos vinculados pelo mesmo grupo.
- Validação de origem e destino pertencentes à empresa ativa.
- Bloqueio de contas e caixas inativas.
- Exigência de sessão aberta quando a origem for caixa.
- Verificação de saldo para caixas que não permitem saldo negativo.
- Gravação atómica: os dois movimentos são confirmados ou nenhum é gravado.

## Base de dados
Esta versão não altera o modelo de dados e não exige nova migration.
