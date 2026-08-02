# FinancePro v2.1.1 — Administration Build Fix

## Correções

- Adicionados os aliases `Ok` e `Fail` a `Result` e `Result<T>`, mantendo compatibilidade com `Success` e `Failure`.
- Corrigida a validação de utilizadores para usar `IsSuccess`.
- Revistos os serviços de Utilizadores, Perfis e Permissões da camada Application.
- Nenhuma alteração no modelo de dados ou nas migrations.

## Validação local

```powershell
dotnet clean .\FinancePro.sln
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
