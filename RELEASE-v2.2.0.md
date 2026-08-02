# FinancePro v2.2.0 — Administração UI

## Entrega

- Integração da interface de Utilizadores com `UserAdministrationService`.
- Integração da interface de Perfis com `ProfileAdministrationService`.
- Integração da matriz de Permissões com `PermissionAdministrationService`.
- Validações e mensagens da camada Application apresentadas diretamente na UI.
- Estados de carregamento e gravação adicionados aos ViewModels.
- Navegação da `MainWindow` atualizada para resolver os novos casos de uso pelo contentor de DI.
- Nenhuma alteração no modelo de dados e nenhuma migration necessária.

## Validação local

```powershell
dotnet clean .\FinancePro.sln
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
