# FinancePro v2.0.4 — WPF Application Namespace Fix

## Correções

- Corrigido o conflito entre o namespace `FinancePro.Application` e o tipo WPF `System.Windows.Application`.
- `LoginView.xaml.cs` passa a usar `System.Windows.Application.Current.Shutdown()`.
- `GestorTema.cs` passa a usar `System.Windows.Application.Current.Resources`.
- Mantidos os quatro testes unitários da camada Application.
- Nenhuma alteração no modelo de dados ou nas migrations.

## Validação local

```powershell
dotnet clean .\FinancePro.sln
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
