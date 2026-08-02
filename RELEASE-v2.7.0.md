# FinancePro ERP v2.7.0 — Orçamento

## Entregas

- Camada `FinancePro.Application.Budget`.
- `BudgetApplicationService` com validações e resultados padronizados.
- Contrato `IBudgetGateway` e adaptador `BudgetGateway`.
- `OrcamentoViewModel` integrado à camada Application.
- Validação de período, ano, linhas orçamentais e revisões.
- Testes unitários para criação e linhas do orçamento.
- Nenhuma alteração no modelo de dados; migration não necessária.

## Validação local

Execute `dotnet clean`, `dotnet restore`, `dotnet build` e `dotnet test` antes do commit.
