# v6.0.0
- Períodos contabilísticos e preparação para fecho mensal.

# v6.71.0 — Totalizadores comerciais da fatura

- Adicionados base tributável, IVA, desconto geral, frete/despesas e total final.
- Valores atualizados automaticamente ao selecionar ou alterar as linhas da fatura.
- Moeda obtida da empresa ativa.
- Viewbox impede o corte de valores elevados.
- Total final destacado visualmente com a cor principal do FinancePro.

# v6.70.1 — Logótipo oficial v4

- Substituída a identidade anterior pelo ficheiro financepro-logo-v4 fornecido pelo utilizador.
- Símbolo compacto extraído diretamente do logótipo v4, sem redesenho.
- Atualizados aplicação, login, menu, configuração, documentos e instalador.
- Recriado o ícone Windows em sete resoluções, de 16 a 256 píxeis.

# v6.70.0 — Nova identidade visual FinancePro

- Aplicado o novo logótipo fornecido pelo utilizador.
- Criada variante compacta e transparente para tamanhos reduzidos.
- Atualizados login, configuração inicial, menu lateral e ícones das janelas.
- Executável e instalador passam a utilizar ícone multirresolução próprio.
- Documentos comerciais impressos passam a incluir o novo símbolo no cabeçalho.

# v6.69.1 — Transações resilientes na faturação

- Corrigida incompatibilidade entre transações manuais e SqlServerRetryingExecutionStrategy.
- Duplicação, faturação definitiva, recebimentos e notas fiscais passam pela estratégia recuperável do SQL Server.
- Recebimentos parciais passam a ser integralmente transacionais.
- Movimento de tesouraria, documento fiscal, stock e contabilização permanecem na mesma unidade atómica.

# v6.69.0 — Resumo operacional da faturação

- Adicionados cartões para documentos em aberto, valor faturado, valor recebido e saldo pendente.
- Indicadores respondem imediatamente aos filtros aplicados na tabela.
- Valores utilizam a moeda configurada para a empresa.
- Valores longos ajustam-se automaticamente para não ficarem ocultos.
- Cartões seguem o padrão visual consolidado do ecrã Empresas.

# v6.68.8 — Correção da pré-visualização WPF

- Corrigido conflito entre o namespace FinancePro.Application e a classe System.Windows.Application.
- A janela de pré-visualização volta a reconhecer corretamente a janela principal como proprietária.
- Mantidas todas as funcionalidades introduzidas na v6.68.7.

# v6.68.7 — Pré-visualização de impressão

- Proformas, faturas, recibos e notas fiscais abrem numa pré-visualização antes da impressão.
- Página configurada em formato A4, com margens próprias para documentos comerciais.
- Visualizador permite navegar entre várias páginas e ajustar a visualização.
- A impressão e a seleção da impressora passam a ser acionadas pela barra do visualizador.
- Janela de pré-visualização abre centralizada sobre o FinancePro.

# v6.68.6 — Documentos comerciais personalizados

- Proforma, fatura, recibo e notas passam a apresentar o nome real da empresa emitente.
- Cabeçalho inclui NIF, morada, telefone e e-mail da empresa quando preenchidos.
- Bloco do cliente inclui NIF, morada, telefone e e-mail disponíveis no cadastro.
- Identificação, emissão e vencimento foram reorganizados num cabeçalho profissional.
- Totais impressos passam a utilizar a moeda configurada para a empresa.

# v6.68.5 — Continuidade do fluxo de recibos

- A fatura permanece selecionada depois do recebimento, mesmo quando deixa de corresponder ao filtro ativo.
- O recibo recém-emitido é selecionado automaticamente na lista de documentos fiscais.
- O valor de recebimento é limpo depois da operação concluída.
- Adicionado acesso direto para imprimir o último recibo da fatura selecionada.
- O botão de impressão fica desativado quando ainda não existe recibo.

# v6.68.4 — Pesquisa e filtros de faturação

- Pesquisa por número de proforma/fatura, descrição ou cliente.
- Filtro por rascunho, proforma validada, definitiva, parcial, paga, vencida ou cancelada.
- Filtro por intervalo de datas de emissão.
- Ação Limpar restaura a lista completa.
- Filtros exclusivos da Faturação; o módulo Receitas permanece inalterado.

# v6.68.3 — Duplicação de documentos

- Proformas e faturas podem originar um novo rascunho através da ação Duplicar.
- Cabeçalho, condições comerciais, totalizadores e linhas são copiados.
- É gerada uma nova numeração e a emissão passa para a data atual.
- Pagamentos, recibos, notas e histórico fiscal não são copiados.
- Operação transacional para impedir documentos parcialmente duplicados.

# v6.68.2 — Edição das linhas da proforma

- Adicionada edição de produto/serviço, quantidade, preço, desconto e IVA das linhas.
- O mesmo formulário de item alterna entre adicionar e guardar alterações.
- Incluída ação para cancelar a edição da linha.
- Total do documento recalculado depois de adicionar, editar ou remover.
- Proteção reforçada na camada de dados: apenas proformas em preparação aceitam alterações nas linhas.

# v6.68.1 — Edição de rascunhos de faturação

- Adicionada edição dos dados gerais das faturas proforma em preparação.
- Cliente, categoria, datas, pagamento, centro de custo, desconto, frete, despesas e observações podem ser atualizados.
- A edição preserva a numeração e recalcula o total a partir das linhas.
- Proformas validadas, faturas definitivas e documentos cancelados permanecem bloqueados.
- Incluída ação para cancelar a edição sem alterar o documento.

# v6.68.0 — Faturação operacional

- A fatura proforma passa a nascer como rascunho, sem exigir valor manual.
- O total é calculado exclusivamente a partir dos itens, desconto geral, frete e outras despesas.
- O novo rascunho é selecionado automaticamente para inclusão imediata das linhas.
- Incluído percurso visual: Rascunho, Proforma, Validada, Definitiva e Recibo.
- Inclusão e remoção de itens ficam disponíveis apenas durante a preparação.
- A validação da proforma exige pelo menos um item e total positivo.
- Mantido o comportamento tradicional do módulo Receitas.

# v6.67.2 — Margens laterais do Dashboard

- Margens laterais do Dashboard ajustadas de 22 px para 24 px.
- Alinhamento exterior uniformizado com os restantes ecrãs, sem alterar cartões ou estrutura.

# v6.67.1 — Correção automática da estrutura de faturação

- Corrigido o erro `Invalid column name` nos campos DescontoGeral, Frete, OutrasDespesas e Observacoes.
- O serviço de faturação passa a confirmar e atualizar a estrutura necessária antes da primeira consulta ou gravação.
- O script foi renumerado para `054_InvoiceManagement.sql` e incluído explicitamente no Bootstrap.
- A validação do Bootstrap confirma agora os quatro campos da gestão documental.

# v6.67.0 — Gestão documental de faturação

- Adotado um ciclo documental explícito: proforma em preparação, proforma validada, fatura definitiva, parcialmente paga e paga/recibo emitido.
- Adicionados desconto geral, frete, outras despesas e observações à faturação.
- Total recalculado por `itens - desconto + frete + outras despesas`.
- Impressões passam a apresentar os novos totalizadores e observações.
- Incluído script incremental de gestão documental para bases existentes.
- Mantidas apenas regras adequadas ao FinancePro e ao uso em FCFA; regras fiscais brasileiras não foram importadas.

# v6.66.1 — Gestão de Faturas e Impressão

- Mantidos separadamente os módulos **Faturação** e **Receitas**.
- Faturação passa a gerir fatura proforma, validação e conversão em fatura definitiva.
- A fatura definitiva pode ser liquidada total ou parcialmente, gerando recibo.
- Adicionada impressão de fatura proforma, fatura definitiva, recibo, nota de crédito e nota de débito.
- Nova numeração de proformas com prefixo `PRO`.

# v6.66.0 — Módulo de Faturação

- Substituído o módulo inativo **Demonstrações Financeiras** por **Faturação**.
- Adicionada navegação e permissão próprias para o módulo `Faturacao`.
- O ecrã de faturação permite gerir propostas, linhas de produtos/serviços, IVA, emissão de faturas, recebimentos, recibos e notas de crédito ou débito.
- Mantida a integração com clientes, stock, tesouraria e contabilização automática.
- Preservado o módulo Receitas como visão operacional das contas a receber.

# v6.65.27 — Ações nas Tabelas

- Adicionada edição a centros de custo, taxas de IVA, plano de contas e sequências documentais.
- Adicionados Editar e Ativar/Desativar aos cadastros administrativos aplicáveis.
- Adicionada edição de departamentos na Contabilidade Analítica.
- Adicionada edição de projetos e ordens de trabalho.
- Adicionada edição de contas bancárias e mantida a ação Ativar/Desativar.
- Adicionado botão Abrir na tabela de orçamentos.
- Relações hierárquicas do plano de contas são preservadas durante a edição.
- Tabelas de auditoria, relatórios e resumos permanecem apenas para consulta.

# v6.65.26 — Centros de Custo da Serralharia

- Registados 14 centros de custo, de CC01 a CC14.
- Incluídas as áreas administrativas, produtivas, comerciais, logísticas e de segurança.
- Centros associados automaticamente a cada empresa.
- Carga idempotente, preservando cadastros existentes e evitando duplicação de códigos.

# v6.65.25 — Plano de Contas da Serralharia

- Registadas cinco classes principais e 28 contas analíticas.
- Estrutura aplicada ao plano orçamental e ao plano contabilístico empresarial.
- Contas sintéticas não aceitam lançamentos; contas analíticas aceitam lançamentos.
- Contas de custos/despesas exigem centro de custo no plano empresarial.
- Carga automática e idempotente, sem duplicar códigos já existentes.

# v6.65.24 — Ajustes visuais da Gestão Orçamental

- Campo Nome reduzido e campos Data início e Data fim ampliados.
- Moeda alterada para ComboBox com opções predefinidas.
- Botões Criar orçamento e Adicionar linha redimensionados e alinhados.
- Coluna Nome e altura das linhas da tabela de orçamentos reduzidas.
- Colunas financeiras da Execução Orçamental distribuídas uniformemente.
- Campo Conta do plano reduzido e campos Centro de custo e Departamento ampliados.
- Nenhuma alteração estrutural à base de dados.

# v6.65.23 — Edição do cadastro de Bancos

- Adicionado botão Editar na tabela de bancos.
- Os dados do banco selecionado são carregados no formulário e podem ser atualizados.
- Incluída opção para cancelar a edição e iniciar um novo cadastro.
- Altura dos campos e botões do formulário reduzida ligeiramente para 38 px.
- Mantida a evolução automática da base de dados da versão anterior.

# v6.65.22 — Informações adicionais dos Bancos

- Adicionados Sigla, Endereço e Contacto ao cadastro de bancos.
- Os novos dados passam a ser apresentados na tabela do catálogo de bancos.
- Sigla e SWIFT são normalizados em maiúsculas e possuem validação de tamanho.
- Incluída evolução automática e idempotente da base de dados, preservando os registos existentes.

# v6.65.21 — Todos os módulos nas Permissões

- A matriz de permissões passa de 16 para 30 módulos.
- Incluídos todos os módulos apresentados no menu lateral.
- A visibilidade de cada módulo passa a respeitar a permissão de consulta.
- O Administrador mantém acesso total automático.
- Nenhuma alteração estrutural à base de dados.

# v6.65.20 — Recorte dinâmico dos cabeçalhos

- Criado recorte superior que recalcula os cantos quando a largura da tabela muda.
- O canto superior direito permanece arredondado ao expandir ou encolher o menu lateral.
- A solução não depende da largura nem da posição da última coluna.
- Mantidos dados, colunas, paginação e restantes estruturas.
- Nenhuma alteração à base de dados.

# v6.65.19 — Colunas da tabela de Utilizadores

- Coluna Foto ampliada de 54 para 64 px e imagem de perfil de 34 para 38 px.
- Redistribuídas proporcionalmente as colunas Nome, Email, Empresa, Perfil e Estado.
- Definidas larguras mínimas para evitar ocultação dos conteúdos.
- Ativada rolagem horizontal automática apenas quando necessária.
- Nenhuma alteração à base de dados.

# v6.65.18 — Colunas da tabela de Permissões

- Mantida a coluna Módulo como coluna diferenciada e mais larga.
- As sete colunas de permissões passam a ter exatamente a mesma dimensão.
- Definidas larguras mínimas para impedir ocultação dos cabeçalhos e controlos.
- Ativada rolagem horizontal automática apenas quando necessária.
- Nenhuma alteração à base de dados.

# v6.65.17 — Distribuição vertical dos cartões

- Títulos posicionados no topo, valores no centro vertical e legendas na base dos cartões.
- Valores mantidos alinhados à esquerda e com redução automática quando extensos.
- Normalizados para 14 px os valores locais antigos dos cartões de Empresas, Exercícios e Moedas.
- Mantida a tipografia 11/14/10 px e o espaçamento horizontal compacto.
- Nenhuma alteração à base de dados.

# v6.65.16 — Cartões KPI compactos

- Tipografia 11/14/10 px aplicada a todos os cartões KPI.
- Títulos apresentados com capitalização convencional e valores seminegritos.
- Margem interna dos cartões reduzida de 16 para 12 px.
- Espaço entre ícone e textos reduzido de 10 para 6 px, mantendo os ícones em 40 × 40 px.
- Preservada a redução automática de valores extensos através do Viewbox.
- Nenhuma alteração à base de dados.

# v6.65.15 — Tipografia dos cartões KPI

- Títulos dos cartões apresentados com capitalização normal em vez de maiúsculas integrais.
- Valor principal reduzido de 22 para 14 px, mantendo peso seminegrito.
- Legendas mantidas em 10 px e ícones mantidos em 18 px dentro de círculos de 40 px.
- Preservada a redução automática de valores extensos através do Viewbox.
- Nenhuma alteração à base de dados.

# v6.65.14 — Cartões no padrão visual de Empresas

- Uniformizados 43 cartões principais em nove ecrãs segundo o padrão visual de Empresas.
- No Dashboard, a alteração foi limitada às duas primeiras linhas de cartões.
- Aplicados ícone circular, hierarquia tipográfica, espaçamentos e dimensões consistentes.
- Preservados bindings, comandos, dados e estruturas funcionais existentes.
- Nenhuma alteração à base de dados.

# v6.65.13 — Ícone do calendário alinhado à direita

- Ícone dos 42 DatePicker alinhado à direita do botão do calendário.
- Adicionado espaçamento interno de 6 px em relação à extremidade direita.
- Mantido o valor da data centralizado.
- Nenhuma alteração à base de dados.

# v6.65.12 — Pesquisas e DatePicker arredondados

- Corrigida a herança do estilo FP.FilterTextBox para garantir cantos de 8 px nos campos de pesquisa.
- Aplicado template arredondado aos 42 DatePicker do sistema.
- Data e ícone do calendário centralizados horizontal e verticalmente.
- Adicionado separador discreto, foco azul e popup de calendário arredondado.
- Preservados bindings, formatos de data e funcionalidades existentes.
- Nenhuma alteração à base de dados.

# v6.65.11 — CheckBox aplicado às tabelas

- Criado estilo específico para os CheckBox gerados por DataGridCheckBoxColumn.
- Aplicado o desenho da referência às 19 colunas booleanas existentes.
- CheckBox centralizado nas células nos modos de leitura e edição.
- Preservados bindings, permissões de edição e estruturas das tabelas.
- Nenhuma alteração à base de dados.

# v6.65.10 — CheckBox alinhado à referência visual

- Ajustado o azul do CheckBox marcado para #2A78D6.
- Reduzido o raio dos cantos para 5 px, mantendo a caixa em 22×22 px.
- Check branco reduzido e recentrado, com traço de 2 px e terminações arredondadas.
- Removido o contorno visível no estado marcado.
- Preservadas as transições, hover e acessibilidade da versão anterior.
- Nenhuma alteração à base de dados.

# v6.65.9 — CheckBox global moderno

- Aplicado a todo o sistema um CheckBox de 22×22 px com cantos de 6 px.
- Estado marcado com fundo azul e check branco centralizado.
- Estado desmarcado transparente com contorno cinza de 1,5 px.
- Transições de 120 ms ao marcar, desmarcar e passar o cursor.
- Feedback de hover com escala suave e suporte ao estado indeterminado.
- Nenhuma alteração à base de dados.

# v6.65.8 — Colunas de Clientes e Fornecedores

- Reduzida de 100 para 72 px a coluna Estado no ecrã Clientes & Fornecedores.
- Ampliada de 100 para 116 px a coluna Ações para apresentar integralmente os dois ícones.
- Preservados os dados, comandos e restantes larguras da tabela.
- Nenhuma alteração à base de dados.

# v6.65.7 — Rodapé único e indicador de página compacto

- Removidos os rodapés duplicados de Empresas, Exercícios Financeiros e Moedas.
- As três tabelas passam a usar diretamente as coleções completas com a paginação global.
- Reduzido de 36×36 para 30×30 px o cartão azul da página atual em todas as tabelas.
- Mantidos maiores, transparentes e sem borda os botões anterior e seguinte.
- Nenhuma alteração à base de dados.

# v6.65.6 — Rodapé global e paginação das tabelas

- Adicionado rodapé global às tabelas com contador automático de registos.
- Implementada paginação reutilizável de 10 registos por página.
- Página atual destacada em azul e comandos anterior/seguinte com estado automático.
- Botões anterior e seguinte ampliados, transparentes e sem bordas.
- Cantos inferiores do rodapé arredondados sem alterar estruturas dos ecrãs.
- Nenhuma alteração à base de dados.

# v6.65.5 — Arredondamento superior dos cabeçalhos

- Aplicado arredondamento exclusivamente nos cantos superiores dos cabeçalhos das tabelas.
- Mantidos retos os cantos inferiores e preservada a faixa contínua sem linhas verticais.
- Nenhuma estrutura, coluna, binding ou funcionalidade foi alterada.
- Nenhuma alteração à base de dados.

# v6.65.4 — Cabeçalhos de tabela sem separadores verticais

- Removidas globalmente as linhas verticais entre as colunas dos cabeçalhos.
- Preservadas as estruturas, larguras, bindings e funcionalidades das tabelas.
- Mantidas apenas as linhas horizontais entre os registos.
- Nenhuma alteração à base de dados.

# v6.65.3 — Ajuste visual do Dashboard

- A coluna Saldo do cartão Saldos por origem passa a reservar 150 px.
- Valores monetários alinhados à direita e apresentados sem truncamento.
- A coluna Origem permanece flexível para ocupar o espaço restante.
- Nenhuma alteração à base de dados.

# v6.65.2 — Padronização global das tabelas

- Todas as tabelas dos ecrãs passam a seguir o padrão visual da tabela de Empresas.
- Unificados cabeçalhos, altura das linhas, separadores, seleção, cores e alinhamento vertical.
- Preservado o comportamento editável das tabelas de Permissões e Consolidação.
- Adicionado gate à auditoria visual para impedir tabelas fora do padrão.
- Nenhuma alteração à base de dados.

# v6.65.1 — Abertura de caixa e alinhamento do Dashboard

- O estado de abertura passa a ser verificado por caixa selecionada, e não globalmente por empresa.
- Permitida a abertura de uma caixa quando outra caixa ou operador já possui sessão ativa.
- Removido o conflito entre transação manual e repetição automática do SQL Server.
- Primeira caixa ativa selecionada automaticamente e sessão do operador validada.
- Cartão Alertas alinhado pela base do cartão Conformidade fiscal.
- Nenhuma alteração à base de dados.

# v6.65.0 — Fotografias de perfil

- Seleção, pré-visualização, substituição e remoção da fotografia do utilizador.
- Redimensionamento e compressão automáticos de imagens JPG e PNG.
- Apresentação da foto na gestão de utilizadores e no cabeçalho da sessão.
- Fallback visual com a inicial do nome quando não existe fotografia.
- Adicionada migração anulável para armazenamento persistente da fotografia.

# v6.64.3 — Correção global de codificação

- Corrigidos textos com mojibake, incluindo `Â`, sequências `Ã...` e travessões corrompidos.
- Preservados os caracteres portugueses legítimos em código, mensagens e documentação.
- Adicionado gate automático de codificação ao processo de validação da release.
- Nenhuma alteração à base de dados.

# v6.64.2 — Correção global de Core.DTOs

- Padronizada a apresentação textual dos DTOs visuais de todos os módulos.
- Eliminados nomes técnicos `FinancePro.Core.DTOs.*` dos controlos WPF.
- Adicionado teste automático por reflexão contra regressões.
- Nenhuma alteração à base de dados.

# v6.64.1 — Correção da apresentação de Empresas e Perfis

- `EmpresaListItemDto` e `PerfilDto` passam a apresentar os respetivos nomes nos controlos WPF.
- Adicionados testes automatizados de representação textual dos DTOs.
- Nenhuma alteração à base de dados.

# v6.64.0 — Acabamento Funcional e Visual de Stocks

- Pesquisa, filtros, edição e arquivo lógico de produtos.
- Exportações CSV do inventário e dos movimentos.
- Novos indicadores operacionais e interface reorganizada.
- Nenhuma alteração à base de dados.

# v6.63.0 — Pipeline Mestre de Release

- Versão única centralizada em `release.json` e aplicada aos assemblies.
- Validação, publicação e instalador executados por um único comando.
- Verificação final dos artefactos, checksums e relatório JSON da release.
- Nenhuma alteração à base de dados.


## v5.4.0 — Document Engine Foundation

- Motor de documentos institucionais.
- Seis modelos iniciais e campos dinâmicos.
- Pré-visualização e exportação TXT/HTML.
- Integração no menu principal e testes automatizados.


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

# v6.62.0 — Instalador Profissional para Windows

- Adicionado instalador Inno Setup 6 para `win-x64`.
- Atalhos, desinstalação, atualização e preservação de configuração.
- Pacote autónomo por padrão e checksum SHA-256.
- Nenhuma alteração à base de dados.

# v6.61.0 — Publicação Automatizada para Windows

- Publicação `win-x64`, `win-x86` e `win-arm64` automatizada.
- Pacote dependente do runtime ou autónomo.
- Manifesto de ficheiros e checksum SHA-256 do ZIP.
- Ajustada a codificação da saída PowerShell.
- Nenhuma alteração à base de dados.

# v6.60.0 — Gate Automatizado de Produção

- Adicionado `Validar-Release.ps1` para restore, Bootstrap, build e testes.
- Relatório JSON por etapa e falha imediata por código de saída.
- Documentação de preparação para produção.
- Nenhuma alteração à base de dados.

# v6.59.0 — Quality Gate do RH Financeiro

- Adicionados 10 testes automatizados dos modelos financeiros de RH.
- Cobertura de remuneração, períodos, férias, empréstimos e regras salariais.
- Total esperado da solução: 74 testes aprovados.
- Nenhuma alteração à base de dados.

# v6.58.0 — Relatórios Consolidados do RH Financeiro

- Resumo de efetivos e custos por departamento.
- Exportações CSV de colaboradores, folha salarial e departamentos.
- Conclusão dos 18 módulos funcionais principais do FinancePro.
- Nenhuma alteração à base de dados.

# v6.57.1 — Correção de Compilação

- Corrigidas duas construções de `DateTime` que provocavam o erro `CS8754` no projeto WPF temporário.
- Nenhuma alteração à base de dados.

# v6.57.0 — Adiantamentos e Empréstimos a Colaboradores

- Gestão de contratos, prestações mensais e saldo devedor.
- Desconto automático na folha e atualização do saldo após aprovação.
- Novas evoluções `HumanResourcesEmployeeLoans`, `HumanResourcesLoanPayments` e `LoanDeduction`.

# v6.56.0 — Impostos e Segurança Social na Folha Salarial

- Regras fiscais e contributivas parametrizáveis por taxa, valor fixo, base e vigência.
- Cálculo automático na geração da folha salarial.
- Detalhe de imposto, contribuição do trabalhador e encargo patronal.
- Nova evolução idempotente `048_HumanResourcesPayrollRules.sql`.

# v6.55.0 — Saldos Anuais de Férias

- Direito anual, saldo transitado, dias utilizados e saldo disponível.
- Dias utilizados calculados automaticamente a partir das férias aprovadas.
- Nova evolução idempotente `047_HumanResourcesLeaveBalances.sql`.

# v6.54.0 — Assiduidade e Horas Extraordinárias

- Controlo diário de entrada, saída e horas trabalhadas.
- Aprovação de horas extraordinárias com integração na folha salarial.
- Nova evolução idempotente `046_HumanResourcesAttendance.sql`.

# v6.53.0 — Férias, Licenças e Faltas

- Gestão de ausências por colaborador com aprovação ou rejeição.
- Férias, licenças e faltas remuneradas ou não remuneradas.
- Descontos aprovados integrados automaticamente na folha salarial.
- Nova evolução idempotente `045_HumanResourcesAbsences.sql`.

# Changelog

## v6.52.3 — Correção de Cores nas Vistas Modulares

- Pincéis locais adicionados a Projetos, Investimentos e RH Financeiro.
- Eliminado o conflito de tipo entre `Style` e `Foreground`.
- Sem alteração de esquema da base de dados.

## v6.52.2 — Correção do Recurso Visual Card

- Alias global `Card` adicionado ao sistema visual.
- Corrigida a abertura de Projetos, Investimentos e RH Financeiro.
- Sem alteração de esquema da base de dados.

## v6.52.1 — Correção de Inicialização dos Módulos

- Bootstrap protegido contra tabelas operacionais opcionais ausentes.
- Menu corrigido para Projetos, Investimentos e RH Financeiro.
- Validação explícita da estrutura necessária aos três módulos.

## v6.52.0 — Componentes Remuneratórios

- Abonos e descontos recorrentes ou temporários por colaborador.
- Subsídios, prémios, horas extraordinárias, impostos e contribuições.
- Integração automática na geração da folha salarial.
- Esquema idempotente `044_HumanResourcesAdjustments.sql`.

## v6.51.0 — Pagamento da Folha Salarial

- Pagamentos individuais e pagamento integral em lote.
- Referência bancária, data e histórico por colaborador.
- Estados automáticos de pagamento da folha.
- Sem alteração de esquema após a v6.50.0.

## v6.50.0 — Processamento da Folha Salarial

- Folha salarial mensal gerada a partir dos colaboradores ativos.
- Totais bruto, descontos, líquido e detalhe individual.
- Aprovação da folha por período.
- Esquema idempotente `043_HumanResourcesPayroll.sql`.

## v6.49.0 — Cadastro e Remuneração de Colaboradores

- Novo módulo RH Financeiro ativado no menu.
- Colaboradores, contratos, centros de custo e dados bancários.
- Salário-base, subsídios, descontos e líquido mensal.
- Esquema idempotente `042_HumanResourcesEmployees.sql`.

## v6.48.0 — Avaliação e Encerramento de Investimentos

- Avaliação intercalar, final e pós-investimento.
- Benefícios, custos, valor residual, objetivos e benefício líquido.
- Encerramento, classificação, recomendações e lições aprendidas.
- Esquema idempotente `041_InvestmentEvaluations.sql`.

## v6.47.0 — Fluxos de Caixa e Viabilidade de Investimentos

- Entradas, saídas, fluxo líquido, VPL e data de payback.
- Distinção entre fluxos previstos e realizados.
- Esquema idempotente `040_InvestmentCashFlows.sql`.

## v6.46.0 — Amortização e Prestações de Investimentos

- Plano automático de amortização, capital, juros e vencimentos.
- Registo de pagamentos, atrasos e atualização do saldo devedor.
- Esquema idempotente `039_InvestmentFinancingPayments.sql`.

## v6.45.0 — Financiamentos e Empréstimos de Investimentos
- Cadastro e controlo de financiamentos associados aos investimentos.
- Esquema idempotente `038_InvestmentFinancing.sql`.

## v6.44.0 — Riscos, Metas e Mitigação de Investimentos
- Gestão de risco, mitigação, ROI-alvo e data-meta.
- Evolução idempotente do esquema sem perda de dados.

## v6.43.0 — Execução e Financiamento de Investimentos
- Execução física, fonte/valor de financiamento e próxima etapa do cronograma.
- Evolução idempotente do esquema sem perda de dados.

## v6.42.0 — Planeamento e Cadastro de Investimentos
- Cadastro, edição, aprovação e arquivo lógico dos investimentos.
- Novo esquema idempotente `035_Investments.sql` aplicado pelo Bootstrap.

## v6.41.0 — Carteira e Rentabilidade de Investimentos
- Novo painel de investimentos com execução, retorno, resultado, ROI e risco.
- Carteira derivada dos projetos existentes, sem duplicação de dados.
- Nenhuma migration necessária.

## v6.40.0 — Teste Integrado de Continuidade
- Execução consolidada dos testes de disponibilidade, RPO, integridade, backup e RTO.
- Resultado detalhado em Configurações, sem restaurar a base.
- Nenhuma migration necessária.

## v6.39.0 — Política de Continuidade
- Frequência de backup, retenção, RPO e RTO configuráveis por empresa.
- Avaliação automática da conformidade com o último backup disponível.
- Nenhuma migration necessária.

## v6.38.0 — Plano de Recuperação e Continuidade
- Geração de documento técnico com backup validado, RPO observado e procedimento de recuperação.
- O plano não executa restauração nem expõe credenciais.
- Nenhuma migration necessária.

## v6.37.0 — Verificação de Integridade do Backup
- Verificação do último backup completo com `RESTORE VERIFYONLY` e checksum.
- Resultado integrado em Configurações, sem restaurar ou alterar a base ativa.
- Nenhuma migration necessária.

## v6.36.0 — Retenção e Limpeza Controlada de Backups
- Retenção configurável dos backups mais recentes.
- Limpeza segura limitada a `FinancePro_*.bak`, com confirmação explícita.
- Resumo de ficheiros eliminados e espaço libertado.
- Nenhuma migration necessária.

## v6.35.0 — Histórico e Controlo de Backups
- Consulta dos últimos backups completos, diferenciais e de log no SQL Server.
- Visualização de data, estado, tamanho, duração e destino em Configurações.
- Tratamento orientado para instalações sem permissão de leitura em `msdb`.
- Nenhuma migration necessária.

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

## v5.7.0
- Calendário Fiscal e obrigações tributárias por empresa.

## v5.8.0
- Centro de conformidade fiscal, KPIs, ações de estado e relatório fiscal.


## v5.9.0
- Dashboard fiscal com taxa de conformidade, vencimentos próximos e obrigações atrasadas.

## v6.1.0
- Motor de lançamentos contabilísticos em partidas dobradas.
- Diário contabilístico, contabilização e estorno.

## v6.3.0 — DRE
- Demonstração de Resultados com comparação entre períodos, margens e exportação CSV.
