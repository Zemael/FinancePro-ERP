
## v3.3.0 — Parceiros de Negócio
- Cadastro mestre unificado de clientes e fornecedores.
- Novo serviço de aplicação, gateway EF Core, ViewModel e View WPF.
- Pesquisa, edição e controlo de estado.
- Sem nova migration.

## v3.2.0 — Cadastros Mestres Financeiros

- Adicionada camada Application para Bancos, Contas Bancárias e Moedas.
- Validações centralizadas e normalização de dados bancários.
- ViewModels migrados para os novos casos de uso.
- Adicionados testes unitários.
- Nenhuma migration necessária.


## [3.1.0] - 2026-08-02

### Adicionado
- Application Service, gateway e testes do cadastro mestre de Empresas.

### Alterado
- `EmpresasViewModel` passa a utilizar a camada Application e o padrão `Result`.

# Changelog

## v2.8.0 — Compras

- Adicionada camada Application para Compras.
- Integrados pedidos, aprovações, rejeições e cancelamentos.
- Adicionados testes unitários do módulo.
- Nenhuma migration nova.


## v2.7.1 — Estabilização SQL Server

- Recuperação automática e orientada quando a ligação SQL falha no arranque.
- Retry do EF Core, timeouts ampliados e pool de conexões limpo.
- Nenhuma migration necessária.


## [2.6.0] - 2026-08-02

### Adicionado
- Camada Application para Despesas.
- Gateway de Despesas na camada Data.
- Integração do ViewModel e testes unitários.
- Atualização manual e mensagens de sucesso na interface.

# Changelog


## [2.4.1] - 2026-08-02

### Alterado
- Alinhamento superior do botão Atualizar e da data/hora no Dashboard.
- Formato completo da última atualização.

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

## v2.3.0 — Dashboard Executivo
- Saudação dinâmica, atualização manual e hora da última atualização.
- Disponibilidade total de caixa e bancos.
- Ações rápidas ligadas aos módulos existentes.
- Melhor tratamento de erros e pequenos ajustes visuais.

## v2.4.0 — Tesouraria Application

- Adicionada camada Application para Tesouraria.
- ViewModel de Tesouraria desacoplado do serviço de dados.
- Adicionados gateway, validações e testes unitários iniciais.
- Nenhuma migration nova.

## v2.5.0 — Receitas

- Introduzida a camada Application para Contas a Receber.
- Interface de Receitas integrada ao serviço de aplicação.
- Validações e mensagens padronizadas com Result.
- Adicionados testes para datas e origem de recebimento.

## v2.6.1 — Login Field Visibility
- Corrigida a visibilidade do texto no email e dos caracteres mascarados na palavra-passe.

## v2.7.0 — Orçamento

- Introduzida a camada Application do módulo Orçamento.
- Orçamento, execução mensal, linhas e revisões passam por `BudgetApplicationService`.
- Adicionados gateway de dados e testes unitários.
- Nenhuma migration necessária.

## v2.9.0 — Património
- Adicionada camada Application para bens patrimoniais.
- Integração do ViewModel com Result/Result<T>.
- Adicionados testes unitários do módulo.

## v3.0.0 — UI Framework
- Adicionado `FP.Components.xaml` com componentes visuais reutilizáveis.
- Padronizados cartões, toolbars, botões, campos, grelhas e mensagens.
- Integrado o novo framework visual nos recursos globais da aplicação.
- Nenhuma migration necessária.

## v3.3.1 — UI Framework Stability
- Corrigidos recursos WPF ausentes na ParceirosView.
- Adicionados aliases FP.PageBackground, FP.PageTitle e FP.PageSubtitle.

## v5.2.0 — Workflow Foundation
- Adicionado motor base de workflow e Centro de Tarefas.
- Incluídas decisões de aprovação, rejeição e devolução.
- Adicionadas tabelas WorkflowTasks e WorkflowHistory.

## v5.3.0 — Reporting Foundation
- Central de Relatórios com pré-visualização e exportação CSV.
- Relatórios iniciais de receber, pagar, tesouraria e património.
- Serviço reutilizável na Platform e provider EF Core na Data.
