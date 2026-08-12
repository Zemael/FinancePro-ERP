# FinancePro v6.9.3 — Otimização e Performance

- Remove `Microsoft.EntityFrameworkCore.Design` do projeto UI; ferramentas de design/migrations permanecem em `FinancePro.Data`.
- Evita que dependências de design do EF/Roslyn sejam carregadas desnecessariamente pela aplicação WPF.
- Mantém a arquitetura, SQL Server, relatórios, consolidação e design visual existentes.
- Adiciona `Package-FinancePro.ps1` para gerar entregas leves com uma única pasta raiz `FinancePro/`.
- O empacotamento exclui `bin`, `obj`, `.vs`, `.git`, `TestResults`, backups, logs e temporários.

## Validação local

```powershell
dotnet clean .\FinancePro.sln
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
