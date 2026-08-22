# FinancePro v6.64.1 — Correção da apresentação de Empresas e Perfis

- Corrigida a apresentação de `EmpresaListItemDto` em listas e caixas de seleção WPF.
- Corrigida a apresentação de `PerfilDto` em listas e caixas de seleção WPF.
- Os controlos passam a mostrar o nome da empresa ou do perfil em vez do nome completo da classe.
- Adicionados testes automatizados para impedir a regressão do problema.
- Nenhuma alteração à base de dados.

## Validação

```powershell
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
