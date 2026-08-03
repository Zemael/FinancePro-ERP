# FinancePro ERP v2.8.0 — Compras

## Entrega

- Camada `FinancePro.Application.Purchasing`.
- `PurchasingApplicationService` com validações e resultados padronizados.
- Contrato `IPurchasingGateway` e adaptador `PurchasingGateway`.
- Integração do `CompraViewModel` com a camada Application.
- Criação, aprovação, rejeição e cancelamento de pedidos.
- Atualização manual da lista e mensagens de sucesso/erro.
- Testes unitários do fluxo de compras.

## Base de dados

Não requer nova migration.
