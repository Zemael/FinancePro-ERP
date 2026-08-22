# FinancePro v6.57.1 — Correção de Compilação do RH Financeiro

- Corrigido o erro `CS8754` nas datas iniciais das regras salariais e dos empréstimos.
- Substituídas construções `new(...)` sem tipo-alvo por `new DateTime(...)` explícito.
- Não altera a base de dados e não requer nova execução do Bootstrap após a v6.57.0.

## Atualização

```powershell
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
