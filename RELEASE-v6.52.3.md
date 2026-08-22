# FinancePro v6.52.3 — Correção de Cores nas Vistas Modulares

- Corrigida a exceção `'System.Windows.Style' is not a valid value for property 'Foreground'`.
- As vistas Projetos, Investimentos e RH Financeiro passam a declarar localmente os pincéis de fundo, texto principal e texto secundário.
- Evitado o conflito entre a chave de estilo global `TextoSecundario` e a utilização dessa chave como cor.
- Não altera a base de dados.

## Atualização

```powershell
dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
