# FinancePro ERP v1.1.4 — Correção consolidada do módulo Caixa

Correções aplicadas:

- Corrigido o `StringFormat` do saldo em FCFA.
- Corrigida a estrutura dos `Border` para manter apenas um filho direto.
- Corrigido o acesso ao utilizador autenticado no `CaixaViewModel`.
- Adicionados os recursos globais ausentes: `TituloModulo`, `TextoSecundario`, `Rotulo`, `Cartao`, `BotaoPrimario` e `BotaoSecundario`.
- Adicionados os brushes de compatibilidade: `FundoJanelaBrush`, `TextoSecundarioBrush` e `AzulPrimarioBrush`.
- Adicionado o estilo `FP.FilterComboBox` usado na matriz de permissões.
- Adicionado o alias `AtivoParaTextoConverter` para telas administrativas antigas.
- Corrigido o layout do cabeçalho do Caixa com duas colunas reais.
- Corrigidos valores `TargetNullValue` com espaços e caracteres especiais.
- Validada a estrutura XML de todos os arquivos XAML do projeto.
- Verificadas todas as referências `StaticResource` e `DynamicResource`; não restaram chaves ausentes na análise estática.

## Teste

```powershell
dotnet clean .\FinancePro.sln
dotnet build .\FinancePro.sln
```

Após o build, caso ainda não tenha aplicado a estrutura do Caixa:

```powershell
dotnet ef migrations add CriarSessoesCaixa `
  --project .\FinancePro.Data\FinancePro.Data.csproj `
  --startup-project .\FinancePro.UI\FinancePro.UI.csproj

dotnet ef database update `
  --project .\FinancePro.Data\FinancePro.Data.csproj `
  --startup-project .\FinancePro.UI\FinancePro.UI.csproj
```
