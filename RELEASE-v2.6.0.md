# FinancePro ERP v2.6.0 — Despesas

## Entregas

- Nova camada `FinancePro.Application.Expenses`.
- Serviço `ExpenseApplicationService` com validações e retornos padronizados.
- Contrato `IExpenseGateway` e adaptador `ExpenseGateway` na camada Data.
- `DespesasViewModel` integrado à camada Application.
- Tratamento consistente de carregamento, criação, pagamento e cancelamento.
- Mensagens de sucesso e erro na interface.
- Botão Atualizar no módulo de Despesas.
- Dois novos testes unitários.

## Base de dados

Nenhuma migration nova é necessária nesta versão.
