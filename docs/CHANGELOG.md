## v4.0.3 — 2026-08-04

- Modernização visual de Compras, Património e Orçamento segundo o Dashboard Design System.
- Inclusão de KPIs e comandos de atualização.
- Padronização de DataGrids, formulários e cartões.

## v4.0.2 — Dashboard Design: Receitas, Despesas e Contabilidade

- Modernização visual dos três módulos no padrão do Dashboard.
- KPIs, cabeçalhos, grelhas e painéis de formulário padronizados.


## v4.0.1 — Dashboard Design System: Bancos, Caixa e Tesouraria

- Interfaces de Bancos, Caixa e Tesouraria alinhadas ao padrão visual do Dashboard.
- Adicionados cartões KPI, cabeçalhos padronizados e indicadores de atualização.
- Mantidas as funcionalidades e integrações existentes.
- Nenhuma migration necessária.

# v4.0.0 — Dashboard Design System

- Clientes e Fornecedores adaptados ao padrão visual do Dashboard.
- Novos cartões de indicadores, cabeçalho, pesquisa, grelha e painel de edição.
- Design System ampliado com estilos reutilizáveis para as próximas Views.
- Nenhuma migration necessária.


## v3.8.0 — Tesouraria Avançada
- Camada Application dedicada à Tesouraria.
- Validações de movimentos, transferências, reforços, sangrias e conciliação.
- ViewModel integrado ao novo serviço e testes unitários.

# v3.5.0
- Núcleo contabilístico: plano de contas, lançamentos e partidas dobradas.

# v3.4.0 — Clientes e Fornecedores separados

- Criados módulos independentes de Clientes e Fornecedores.
- Adicionados serviços Application, gateways EF Core, ViewModels e Views separados.
- Integrações existentes de Receitas com Clientes e de Compras/Despesas com Fornecedores foram preservadas.
- Não é necessária nova migration: as tabelas Clientes e Fornecedores já existiam.

# Changelog

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

## v3.6.0 — Contas a Receber

- Criada a camada Application dedicada a Contas a Receber.
- Adicionados resumo operacional, pesquisa e filtro por estado.
- Centralizadas validações de criação, recebimento e cancelamento.
- Mantida a integração com Tesouraria e a estrutura atual da base de dados.

## v3.7.0 — Contas a Pagar

- Adicionada camada Application e gateway dedicado para Contas a Pagar.
- Centralizadas validações de criação, pagamento e cancelamento.
- Adicionados indicadores e testes unitários.
- Integrada a interface de Despesas ao novo serviço de aplicação.

## v3.8.1 — Correção XAML

- Corrigidos `MainWindow.xaml` e `DespesasView.xaml`.
- Todos os ficheiros XAML foram validados estruturalmente.
- Nenhuma migration necessária.

## v3.9.0 — Platform Services
- Adicionado Financial Engine transversal.
- Adicionado serviço central de numeração transacional.
- Adicionados auditoria transversal e eventos de domínio.
- Adicionado script 012_PlatformServices.sql.
