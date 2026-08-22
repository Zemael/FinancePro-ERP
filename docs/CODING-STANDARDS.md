# Coding Standards

- C# com nullable reference types ativados.
- Classes e membros públicos em PascalCase; variáveis locais em camelCase.
- Métodos assíncronos terminam em `Async`.
- Uma classe principal por ficheiro.
- A UI não deve criar ou consultar diretamente o DbContext.
- Casos de uso retornam `Result` ou `Result<T>` quando houver falha esperada de negócio.
- Não ocultar exceções inesperadas; registá-las e apresentar mensagem apropriada.
- Toda alteração de modelo exige migration EF Core própria, exceto quando o modelo não mudou.

## Padrões visuais (XAML)

Regras fixadas depois de várias rondas de correções manuais ecrã a ecrã — para não
se repetirem, todo o XAML novo (ou editado) deve respeitar isto à partida:

- **Cores**: nunca usar `Background="#RRGGBB"` fixo. Usar sempre `{DynamicResource ...}`
  com as chaves de `Theme.Light.xaml`/`Theme.Dark.xaml` (ou os aliases `FP.*`), para
  o tema claro/escuro funcionar. Exceção deliberada: `LoginView`, `SetupView` e a
  sidebar do `MainWindow` (marca fixa, não mudam com o tema).
- **Botões**: nunca definir `Width` fixo — o texto define o comprimento. A altura
  vem sempre do estilo (`BaseButton`/`FP.PrimaryButton`/`FP.SecondaryButton` = 40px),
  nunca `Height` local. Um botão dentro de `StackPanel` vertical precisa de
  `HorizontalAlignment="Left"` explícito, senão estica à largura do painel.
- **Tabelas (`DataGrid`)**: colunas com `Width="Auto"` por omissão (ajusta ao texto);
  só a coluna "principal" (nome/descrição) deve usar `Width="*"`. Nunca redefinir
  `DataGridCell`/`DataGridColumnHeader`/`DataGrid` localmente — os estilos globais em
  `Styles.xaml` já cobrem toda a app.
- **Estilos partilhados**: nunca duplicar `Cartao`/`Card`/`FP.Card`/`Rotulo`/
  `BotaoPrimario`/`BotaoSecundario`/`TituloModulo`/`TextoSecundario` em
  `UserControl.Resources` de uma view — usar sempre as versões globais de
  `FP.DesignSystem.xaml`. Um estilo local com o mesmo nome esconde o global e foge
  ao tema.
- **Texto**: rótulos e legendas (`Rotulo`, `KpiLabel`, `TextoSecundario`, `Muted`)
  têm sempre `TextWrapping="Wrap"` — podem quebrar linha. Valores numéricos (KPIs)
  nunca quebram linha; usar `<Viewbox StretchDirection="DownOnly">` à volta do
  `TextBlock` para encolher a letra em vez de cortar o valor.
- **Antes de dar um módulo como fechado**, correr `Auditoria-Visual.ps1` (raiz do
  projeto) e resolver o que ele assinalar.
