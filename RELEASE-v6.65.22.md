# FinancePro v6.65.22 — Cadastro de Bancos

## Alterações

- Novo campo **Sigla**, com máximo de 20 caracteres e normalização para maiúsculas.
- Novo campo **Endereço**, com máximo de 250 caracteres.
- Novo campo **Contacto**, com máximo de 50 caracteres.
- Formulário e tabela de bancos atualizados para apresentar os novos dados.
- Entidade, DTOs, aplicação, persistência e modelo EF Core atualizados.
- Evolução idempotente `051_BankRegistrationFields.sql` incluída no Bootstrap.

## Validação no Windows

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
