# FinancePro v6.3.1 — Estabilização

## Correções

- Atualizado o `FakeStore` de `ChartAccountAndSequenceTests` para implementar integralmente os métodos de períodos contabilísticos.
- Corrigida a inicialização de `PreviousReportFrom` em `AccountingViewModel`, removendo o erro CS8754 de inferência de tipo.
- Validada a sincronização de `IAdministrationMasterDataStore` com as implementações de testes e SQL.
- Validada a estrutura XML dos ficheiros XAML e a ausência de marcadores de conflito.

## Base de dados

Nenhuma alteração adicional. Permanecem válidos os scripts existentes até `019_AccountingEntryEngine.sql`.
