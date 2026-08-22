# FinancePro v6.57.0 — Adiantamentos e Empréstimos a Colaboradores

- Cadastro de adiantamentos, empréstimos e apoios por colaborador.
- Valor original, saldo devedor, prestação mensal e primeira dedução.
- Ativação e suspensão do desconto sem eliminar o contrato.
- Prestação incluída automaticamente na folha salarial mensal.
- Saldo atualizado apenas após aprovação da folha.
- Liquidação automática quando o saldo chega a zero.
- Novas tabelas idempotentes `HumanResourcesEmployeeLoans` e `HumanResourcesLoanPayments`.

## Atualização obrigatória

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
