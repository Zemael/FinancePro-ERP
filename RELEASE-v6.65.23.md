# FinancePro v6.65.23 — Edição de Bancos

## Alterações

- Função **Editar banco** adicionada à coluna Ações.
- Carregamento de Nome, Sigla, SWIFT, Endereço e Contacto no formulário.
- Atualização segura do registo existente, incluindo validação de nomes duplicados.
- Botão para cancelar a edição e voltar ao modo de novo cadastro.
- Campos e botões do formulário ajustados para 38 px de altura.

## Validação no Windows

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
