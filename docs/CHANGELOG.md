# Changelog


## v2.1.3 — GitHub Publish Fix

- Corrigidos os workflows `ci.yml` e `release.yml` para restaurar os assets específicos de `win-x64` durante o publish.
- Eliminado o erro `NETSDK1047` provocado por `dotnet publish --no-restore --runtime win-x64`.
- Nenhuma alteração no modelo de dados.

## [2.1.2] - 2026-08-02

### Corrigido
- Testes de Utilizadores e Perfis alinhados com a propriedade `IsSuccess` do Result Pattern.
- Aviso de nulabilidade no serviço de utilizadores.


## v2.1.1 — Administration Build Fix

- Compatibilidade `Ok`/`Fail` no Result Pattern.
- Correção de `IsSuccess` no serviço de utilizadores.
- Revisão dos serviços de Administração.


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

## v2.1.0 — Administração Application Layer
- Adicionados casos de uso de Utilizadores, Perfis e Permissões.
- Adicionado adaptador administrativo na camada Data.
- Adicionados testes unitários da camada Application.
- Sem alteração do esquema da base de dados.

## v2.2.0 — Administração UI

- Interface de Utilizadores integrada à camada Application.
- Interface de Perfis integrada à camada Application.
- Interface de Permissões integrada à camada Application.
- Mensagens e validações padronizadas com `Result` e `Result<T>`.
- Navegação administrativa atualizada no shell principal.
