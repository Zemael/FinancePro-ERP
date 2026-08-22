# FinancePro v6.64.2 — Correção global de Core.DTOs

- Corrigida a apresentação textual de todos os DTOs utilizados em listas, seleções e caixas de combinação WPF.
- Empresas, perfis, utilizadores, bancos, contas, caixas, stocks, parceiros, compras, orçamento, tesouraria, património, documentos e configurações passam a apresentar informação funcional.
- Eliminada a apresentação de nomes técnicos como `FinancePro.Core.DTOs.*` na interface.
- Adicionado teste automático por reflexão para proteger todos os DTOs visuais atuais e futuros.
- Nenhuma alteração à base de dados.

## Validação

```powershell
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
