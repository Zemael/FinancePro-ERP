
## [2.0.4] - 2026-08-02

### Corrigido
- Qualificação explícita de `System.Windows.Application.Current` em `LoginView` e `GestorTema`.
- Conflito de namespace introduzido pela camada `FinancePro.Application`.


## v2.0.3 — 2026-08-02

- Corrigida a resolução dos atributos `Fact` nos testes xUnit.
- Corrigido conflito de nomes entre `FinancePro.Application` e `System.Windows.Application`.
- Sem alterações ao esquema da base de dados.

# Changelog

## 2.0.0-foundation — 2026-08-02

### Adicionado
- Projeto `FinancePro.Application`.
- `Result` e `Result<T>`.
- Contratos `INavigationService`, `IDialogService`, `INotificationService`, `ICurrentUserService` e `IDateTimeProvider`.
- Implementações WPF iniciais e registo no contentor DI.
- Documentação inicial em `/docs`.

### Preservado
- Migration consolidada `InitialFinancePro`.
- Fluxos e serviços existentes.
- Estrutura visual atual.

### Nota
A navegação legada da `MainWindow` ainda permanece ativa. A migração para `INavigationService` será incremental na release seguinte para reduzir risco de regressão.

## v2.0.1 — GitHub Engineering Foundation

- GitHub Actions para build WPF em Windows.
- Verificação das migrations do Entity Framework Core.
- Publicação automática do artefacto win-x64.
- Workflow de release por tags semânticas.
- Templates de Pull Request e Issues.
- EditorConfig e guia de configuração do repositório.

## v2.0.2 — Engineering Foundation

- Adicionado projeto de testes da camada Application.
- CI passou a executar testes e publicar resultados/cobertura.
- Adicionados padrões globais de build e seleção do SDK.
- Adicionadas políticas de contribuição, segurança, conduta e branches.
