# FinancePro v6.49.0 — Cadastro e Remuneração de Colaboradores

- Ativação do módulo RH Financeiro no menu principal.
- Cadastro completo de colaboradores e dados contratuais.
- Departamento, centro de custo, função e situação laboral.
- Salário-base, subsídios, descontos e líquido mensal automático.
- Dados bancários, NIF e Segurança Social.
- Indicadores da força de trabalho e custo remuneratório mensal.
- Nova tabela idempotente `HumanResourcesEmployees`, aplicada pelo Bootstrap.

## Atualização

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
