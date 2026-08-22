# FinancePro v6.52.1 — Correção de Inicialização dos Módulos

- Corrigido o script `034_ProjectOperations.sql`, que interrompia o Bootstrap quando tabelas operacionais opcionais não existiam.
- Projetos, Investimentos e RH Financeiro passam a aparecer juntos como botões ativos no menu principal.
- Removida a entrada visual desativada e duplicada de Investimentos.
- Adicionada mensagem clara caso a abertura de algum dos três módulos falhe.
- O comando `--validate` agora confirma explicitamente as tabelas essenciais desses módulos.

## Atualização obrigatória

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
