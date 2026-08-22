# FinancePro v6.65.2 — Padronização global das tabelas

## Alterações

- Aplicado o estilo `FP.DataGrid`, baseado no ecrã de Empresas, a todas as tabelas da aplicação.
- Centralizados cabeçalhos, linhas, grelha horizontal, seleção, cores, tipografia e alinhamento.
- Removidas substituições visuais locais que causavam diferenças entre módulos.
- Mantida a edição nas tabelas de Permissões e Consolidação.
- A auditoria visual passa a detetar tabelas sem o estilo global e propriedades locais incompatíveis.

## Base de dados

Esta versão não requer migração nem alteração da base de dados.

## Validação no Windows

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
