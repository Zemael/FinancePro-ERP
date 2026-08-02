# FinancePro ERP v2.5.0 — Receitas

## Entregas

- Camada `FinancePro.Application` para Contas a Receber.
- Gateway desacoplado entre Application e Data.
- Validação centralizada de emissão, vencimento, valor e origem do recebimento.
- Integração da interface de Receitas com `Result` e `Result<T>`.
- Atualização manual da lista de contas a receber.
- Recebimentos continuam a gerar movimentos na Tesouraria.
- Dois novos testes unitários.

## Base de dados

Esta versão não altera o modelo de dados e não exige nova migration.
