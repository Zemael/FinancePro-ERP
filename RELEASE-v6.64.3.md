# FinancePro v6.64.3 — Correção global de codificação

- Corrigidos artefactos de codificação em mensagens, comentários, testes e scripts.
- Normalizados símbolos como ponto médio, travessão e caracteres portugueses.
- Adicionado `Validar-Codificacao.ps1` ao gate de release para impedir regressões.
- Mantidas as correções globais de apresentação de `Core.DTOs` da v6.64.2.
- Nenhuma alteração à base de dados.

## Validação

```powershell
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
