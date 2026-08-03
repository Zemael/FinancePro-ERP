# FinancePro ERP v2.4.0 — Tesouraria Application

## Entregas

- Criação do caso de uso `TreasuryApplicationService`.
- Criação do contrato `ITreasuryGateway`.
- Adaptador `TreasuryGateway` na camada Data.
- Integração do `TesourariaViewModel` com a camada Application.
- Retornos padronizados com `Result` e `Result<T>`.
- Validação de movimentos, transferências e conciliação.
- Dois testes unitários iniciais da Tesouraria.
- Nenhuma alteração no modelo de dados e nenhuma migration nova.

## Validação necessária

Executar localmente:

```powershell
dotnet clean .\FinancePro.sln
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
