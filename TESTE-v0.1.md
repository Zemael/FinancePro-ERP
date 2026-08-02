# FinancePro ERP — pacote de teste v0.1

## Alterações desta entrega

- Cadastro de Empresas integrado ao menu lateral.
- Listagem e pesquisa por nome, NIF ou email.
- Criação e edição de empresa.
- Ativação e desativação de empresa.
- Serviço `IEmpresaService` registado na injeção de dependência.
- Direitos autorais reservados no ficheiro `COPYRIGHT.txt`.

## Como testar

1. Abra `FinancePro.sln` no Visual Studio 2022.
2. Defina `FinancePro.UI` como projeto de inicialização.
3. Confirme a ligação em `FinancePro.UI/appsettings.json`.
4. Compile a solução.
5. Execute o sistema e faça login.
6. Clique em **Empresas** no menu lateral.
7. Teste pesquisa, criação, edição e ativação/desativação.

## Observação

A estrutura da tabela `Empresas` já existia no projeto original; esta entrega não exige uma nova migration.

## Atualização visual v0.2
- MainWindow redesenhada: sidebar, topbar, perfil e statusbar.
- Dashboard redesenhado com KPIs, ações rápidas, fluxo de caixa, execução orçamental, alertas e painéis operacionais.
- Direitos autorais exibidos no rodapé.
