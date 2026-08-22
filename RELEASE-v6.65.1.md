# FinancePro v6.65.1 — Abertura de caixa e Dashboard

- Corrigido definitivamente o bloqueio de `Abrir Caixa` causado por sessões de outras caixas da empresa.
- Cada caixa passa a consultar e controlar apenas a sua própria sessão operacional.
- A primeira caixa ativa é selecionada automaticamente.
- Removida a transação manual incompatível com `EnableRetryOnFailure`; o EF Core mantém a gravação atómica.
- Adicionada validação da sessão do utilizador e mensagens de erro mais claras.
- O cartão `Alertas` termina agora na mesma linha inferior do cartão `Conformidade fiscal`.
- Nenhuma alteração à base de dados.

## Validação

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
