# FinancePro v6.65.0 — Fotografias de perfil

- Habilitada a fotografia de perfil dos utilizadores.
- Aceita imagens JPG e PNG, redimensionadas para até 512 px e comprimidas automaticamente.
- A foto pode ser selecionada, substituída ou removida no módulo Utilizadores.
- A fotografia do utilizador autenticado aparece imediatamente no cabeçalho do FinancePro.
- Quando não existe fotografia, o sistema apresenta a inicial do nome.
- Incluída a migração `UserProfilePhoto`, sem perda ou alteração de dados existentes.

## Validação

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
