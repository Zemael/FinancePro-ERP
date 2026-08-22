# FinancePro v6.65.6 — Rodapé global e paginação das tabelas

## Alterações visuais e funcionais

- Todas as tabelas que utilizam o padrão `FP.DataGrid` recebem um rodapé uniforme.
- O lado esquerdo apresenta automaticamente a quantidade total de registos carregados.
- O lado direito apresenta página anterior, página atual e página seguinte.
- A página atual utiliza fundo azul, enquanto os botões laterais são maiores, transparentes e sem bordas.
- Os botões anterior e seguinte são automaticamente desativados nos limites da paginação.
- O rodapé tem apenas os cantos inferiores arredondados e completa visualmente o cartão da tabela.
- A paginação apresenta 10 registos por página sem alterar as definições das colunas ou os ecrãs.

## Base de dados

Esta versão não requer migração nem alteração da base de dados.

## Validação no Windows

```powershell
dotnet restore .\FinancePro.sln
dotnet run --project .\FinancePro.Bootstrap -- --validate
dotnet build .\FinancePro.sln --configuration Release
dotnet test .\FinancePro.sln --configuration Release
```
