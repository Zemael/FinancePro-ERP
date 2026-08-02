# FinancePro ERP v0.6 — Cadastros Mestres

## Novos módulos
- Exercícios Financeiros
- Moedas

## Preparar a base de dados
Escolha uma das opções:

1. Entity Framework:
   dotnet ef migrations add CadastrosMestres --project FinancePro.Data --startup-project FinancePro.UI
   dotnet ef database update --project FinancePro.Data --startup-project FinancePro.UI

2. SQL manual:
   Execute `FinancePro.Data/Scripts/Schema/009_CadastrosMestres.sql` no SQL Server.

## Teste
1. Abra o FinancePro e faça login.
2. Abra **Moedas** e confirme XOF, EUR e USD (quando usou o script SQL).
3. Crie ou edite uma moeda.
4. Abra **Exercícios Financeiros**.
5. Selecione uma empresa, informe o ano e guarde.
6. Marque um exercício como padrão; os outros exercícios da mesma empresa devem perder essa marcação.
7. Pesquise, edite e desative registos.
