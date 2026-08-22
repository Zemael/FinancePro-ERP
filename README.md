# FinancePro ERP

Aplicação desktop de gestão financeira em C# / .NET 8 (WPF), com acesso a
dados via Entity Framework Core (Code First) sobre SQL Server.

## Arquitetura (4 projetos)

```
FinancePro
│
├── FinancePro.UI          → WPF (MVVM) e composition root.
├── FinancePro.Application → contratos e resultados dos casos de uso.
├── FinancePro.Core        → entidades, interfaces e DTOs de negócio.
└── FinancePro.Data        → EF Core, persistência e serviços existentes.
```

Dependências entre projetos:

```
FinancePro.UI ──▶ FinancePro.Application ──▶ FinancePro.Core
     ├──────▶ FinancePro.Data ───────────────▶ FinancePro.Core
     └───────────────────────────────────────▶ FinancePro.Core
```

Dentro de `FinancePro.Data`:

```
Context/          → FinanceProDbContext
Configurations/    → Fluent API (uma classe por entidade)
Services/
   Interfaces/      → contratos dos serviços de aplicação
   Implementations/ → regras de negócio de cada módulo
Scripts/
   Schema/          → DDL de referência (schema espelha o Code First)
   Seed/            → dados iniciais de referência
Migrations/         → geradas pelo `dotnet ef migrations add`
```

## Roteiro

- [x] **Etapa 1** — Estrutura do projeto (3 projetos)
- [x] **Etapa 2** — Base de dados: Utilizadores, Perfis, Empresa, Banco,
      ContaBancaria, Caixa, PlanoContas, Categoria, Fornecedor, Cliente
- [x] **Etapa 3** — Ecrã de Login
- [x] **Etapa 4** — Dashboard com cartões (Saldo em Caixa, Saldo Bancário,
      Receitas, Despesas, Resultado, Movimentos Recentes)
- [ ] **Etapa 5** — Módulos (um de cada vez):
      - [x] Dashboard (Etapa 4)
      - [ ] Cadastros Gerais
      - [x] Gestão Orçamental (Orcamento/OrcamentoDetalhe/RevisaoOrcamental — abas Receitas Previstas/Despesas/Execução/Revisões/Relatórios)
      - [x] Tesouraria (registo de movimentos + tabela `Movimentos`)
      - [x] Receitas (contas a receber ligadas a Clientes; ao receber, gera Movimento de Tesouraria)
      - [ ] Despesas
      - [ ] Compras
      - [ ] Gestão Patrimonial
      - [ ] Stocks
      - [ ] Investimentos
      - [ ] Recursos Humanos (Financeiro)
      - [ ] Contabilidade
      - [ ] Demonstrações Financeiras
      - [ ] Planeamento Financeiro
      - [ ] Relatórios
      - [ ] Auditoria
      - [x] Configurações (assistente de 1ª utilização + gestão de Empresa/Utilizadores)
      - [ ] Administração

      Já construídos mas fora desta lista nomeada (fazem parte da tesouraria/cadastros):
      **Caixa** (gestão de caixas) e **Bancos** (catálogo de bancos + contas
      bancárias) — mantidos como estão; podem ser reagrupados dentro de
      Cadastros Gerais mais tarde, se preferir.


## Integração contínua

Pull Requests para `develop` ou `main` executam automaticamente restore, build, testes, validação do Entity Framework e publicação WPF no GitHub Actions. Consulte `docs/engineering/CI-CD.md`.

## Estilo visual

Todos os controlos (Button, TextBox, PasswordBox, ComboBox, DataGrid) usam
um estilo plano/moderno definido centralmente em
`FinancePro.UI/Resources/Styles.xaml` (fundido em `App.xaml`). O menu
lateral tem ícones vetoriais próprios (sem ficheiros de imagem externos).

## Requisitos para abrir/compilar

- Visual Studio 2022/2026 com a carga de trabalho **.NET Desktop
  Development** (necessária para WPF)
- .NET 8 SDK
- SQL Server (LocalDB, Express ou completo) — string de ligação em
  `FinancePro.UI/appsettings.json`

## Como correr pela primeira vez

1. Abra `FinancePro.sln`, defina `FinancePro.UI` como Startup Project.
2. Ajuste a connection string em `FinancePro.UI/appsettings.json` se
   necessário.
3. Crie a base de dados:
   ```
   dotnet ef migrations add InicialCreate --project FinancePro.Data --startup-project FinancePro.UI
   dotnet ef database update --project FinancePro.Data --startup-project FinancePro.UI
   ```
4. **F5.** Sem nenhum utilizador na base de dados, abre o assistente
   **Configuração Inicial** (cria a empresa + o Administrador). Os
   perfis padrão são criados automaticamente pelo assistente.
5. Depois disso, a app abre sempre no Login.

## Atualização: Padrão de ecrã (Toolbar/Filtros/Detalhes), tema claro/escuro

**Novo padrão de ecrã** (piloto no módulo Caixa): Toolbar (Novo · Editar
· Guardar · Eliminar · Exportar, todos funcionais), Filtros e pesquisa,
DataGrid principal, e um painel com 5 abas — **Detalhes** (formulário
real), **Histórico**/**Anexos**/**Auditoria** (placeholders "Em breve" —
não existe ainda sistema de anexos nem log de auditoria), e **Campos
Propostos** (tabela de referência com a especificação do documento
fornecido). "Eliminar" nunca apaga fisicamente um registo financeiro —
desativa, preservando o histórico de movimentos já ligados.

**Tema claro/escuro**: interruptor (☾) na barra superior, visível em
qualquer ecrã. `Theme.Light.xaml`/`Theme.Dark.xaml` definem a paleta;
`Styles.xaml` e o `MainWindow` usam `DynamicResource` para atualizar em
tempo real. Por agora o retrofit de cores está completo no "chrome"
global (sidebar, barra superior, botões, campos, tabelas) e no piloto
(Caixa) — os outros módulos ganham o tema à medida que forem
redesenhados para o novo padrão de ecrã.

## Atualização: CommunityToolkit.Mvvm, permissões e auditoria real

A partir do módulo **Gestão Patrimonial**, os novos módulos usam
`CommunityToolkit.Mvvm` (`[ObservableProperty]`/`[RelayCommand]`) em vez
do `ViewModelBase` manual — os módulos já existentes mantêm-se como
estão, para não haver uma reescrita não planeada.

Infraestrutura nova, reutilizável por qualquer módulo:
- **`SessaoAtual`** (`FinancePro.UI/Common`) — guarda o utilizador ligado
  (definida em `App.xaml.cs` após o login); `SessaoAtual.PodeEliminarOuAprovar`
  é `true` só para Administrador/Gestor
- **`LogAuditoria`** + `IAuditoriaService` — log genérico (Entidade,
  RegistoId, Ação, Utilizador, Data); qualquer módulo pode chamar
  `RegistarAsync(...)` e mostrar `ListarPorRegistoAsync(...)` na aba
  "Auditoria"
- A aba **"Campos Propostos"** agora mostra o **tipo real da coluna**
  (`NVARCHAR(100)`, `DECIMAL(18,2)`, etc.), corrigido também no Caixa

### Migração necessária

Tabelas novas (`Bens`, `LogsAuditoria`):
```
dotnet ef migrations add GestaoPatrimonialEAuditoria --project FinancePro.Data --startup-project FinancePro.UI
dotnet ef database update --project FinancePro.Data --startup-project FinancePro.UI
```

## Próximo passo — roteiro dos 18 módulos (documento de referência)

O utilizador forneceu um documento de especificação com os campos
propostos para os 18 módulos do FinancePro ERP. Progresso:

- [x] 00/01 Dashboard
- [x] 02 Cadastros Gerais (parcial — via Configurações: Empresa/Utilizadores; Caixa/Bancos à parte)
- [x] 03 Gestão Orçamental
- [x] 04 Tesouraria
- [x] 05 Receitas
- [x] 06 Despesas
- [x] 07 **Compras** (novo — `Compra`: Nº Pedido automático, Fornecedor,
      Departamento/Centro de Custo/Projeto/Comprador em texto livre,
      Prioridade, fluxo Aprovar/Rejeitar/Cancelar)
- [x] 08 **Gestão Patrimonial** (novo — `Bem`: Código automático, Nº
      Patrimonial, depreciação linear calculada ao vivo; primeiro módulo
      com CommunityToolkit.Mvvm, permissões e auditoria real)
- [ ] 09 Stocks
- [ ] 10 Investimentos
- [ ] 11 Recursos Humanos (Financeiro)
- [ ] 12 Contabilidade
- [ ] 13 Demonstrações Financeiras
- [ ] 14 Planeamento Financeiro
- [ ] 15 Relatórios
- [ ] 16 Auditoria
- [ ] 17 Configurações (parcial — falta alinhar aos campos completos do documento)
- [ ] 18 Administração

**Nota**: os campos de Tesouraria/Receitas/Despesas construídos ainda
não têm tudo o que o documento pede (ex.: IVA, Retenção, Documento,
Beneficiário, Moeda, Taxa de Câmbio) — fica para uma passagem de
alinhamento depois dos módulos novos estarem todos criados.

## Atualização: Tesouraria real, Despesas nova, Receitas completa

**Tesouraria** — `Movimento` ganhou o modelo completo pedido: 7 tipos de
operação (Entrada, Saída, Transferência, Sangria, Reforço, Ajuste,
Bloqueio), Forma de Pagamento, Centro de Custo, Rubrica (reaproveita a
Categoria já existente), Estado e Conciliado (com botão para marcar/
desmarcar na lista). Transferência/Sangria/Reforço pedem origem **e**
destino e geram duas linhas ligadas (uma Saída, uma Entrada) — o dinheiro
nunca desaparece de um lado sem aparecer no outro. Bloqueio fica
registado mas não afeta nenhum saldo.

**Bloqueio de saldo negativo**: `Caixa` ganhou `PermiteSaldoNegativo`
(checkbox no ecrã de Caixa). Se desligado, qualquer Saída/Transferência/
Sangria que deixasse a caixa negativa é recusada com uma mensagem clara.

**Despesas** — módulo novo (`ContaPagar`), espelho exato de Receitas:
Código automático (`DESP-00001`), Fornecedor, Forma de Pagamento, Centro
de Custo, Rubrica, Estado (Pendente/Paga/Cancelada). "Pagar" gera um
Movimento de Saída — e é bloqueado automaticamente se a caixa de destino
não permitir saldo negativo.

**Receitas** — ganhou Código automático (`REC-00001`), Forma de
Pagamento e Centro de Custo.

**Simplificação assumida**: "Conta/Caixa destino" das contas a
receber/pagar continua a ser escolhida no momento de Receber/Pagar (não
no momento de criar a conta) — mantém a flexibilidade de decidir só
quando o dinheiro entra/sai de verdade.

### Migração necessária

Muitas tabelas mudaram (`Movimentos`, `Caixas`, `ContasReceber`) e uma
é nova (`ContasPagar`):
```
dotnet ef migrations add TesourariaCompleta --project FinancePro.Data --startup-project FinancePro.UI
dotnet ef database update --project FinancePro.Data --startup-project FinancePro.UI
```

### Migração necessária

Tabela nova (`Compras`):
```
dotnet ef migrations add ModuloCompras --project FinancePro.Data --startup-project FinancePro.UI
dotnet ef database update --project FinancePro.Data --startup-project FinancePro.UI
```

## Atualização: Logótipo

O logótipo oficial está em `FinancePro.UI/Resources/Images/financepro-logo-v4.png`,
com o símbolo compacto em `financepro-mark-v4.png` e o ícone multirresolução em `app-v4.ico`.
Aparece no login, na Configuração Inicial, no menu lateral, nas janelas, nos documentos comerciais e no instalador.
Nenhuma migração necessária — só ficheiros de imagem e XAML.

## Atualização: Gestão Orçamental

Módulo novo com 3 tabelas (`Orcamentos`, `OrcamentoDetalhes`,
`RevisoesOrcamentais`) e 5 separadores no ecrã: Receitas Previstas,
Despesas, Execução (Plano Mensal), Revisões, Relatórios. O **Valor
Realizado** nunca é guardado — é calculado ao vivo a partir dos
`Movimentos` já lançados (ligados à mesma conta do Plano de Contas via
Categoria), para nunca ficar desalinhado com a Tesouraria real.

**Simplificação assumida**: Centro de Custo e Departamento ainda não são
cadastros próprios neste projeto — ficam como texto livre em cada linha
do orçamento, até esses módulos existirem.

Como há tabelas novas, é preciso mais uma migração:
```
dotnet ef migrations add GestaoOrcamental --project FinancePro.Data --startup-project FinancePro.UI
dotnet ef database update --project FinancePro.Data --startup-project FinancePro.UI
```

## Atualização: Dashboard rico + Caixa.SaldoMinimo

O Dashboard foi reconstruído: cabeçalho (Empresa/Exercício/Pesquisa
Global/Notificações), cartões por área (Tesouraria/Receitas/Despesas
reais; Orçamento/Património como placeholder — módulos ainda não
existem), indicadores (Resultado, Margem %), Saldo por Conta/Caixa,
Pendências (contas a receber), Alertas reais (caixa abaixo do mínimo,
conta bancária negativa, contas em atraso, lembrete de backup) e Ações
Rápidas. Tamanhos de letra reduzidos em toda a app.

A entidade `Caixa` ganhou um campo novo (`SaldoMinimo`, opcional) — por
isso é preciso gerar **outra migração** antes de correr:
```
dotnet ef migrations add DashboardESaldoMinimo --project FinancePro.Data --startup-project FinancePro.UI
dotnet ef database update --project FinancePro.Data --startup-project FinancePro.UI
```
