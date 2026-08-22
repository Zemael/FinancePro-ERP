# FinancePro v6.65.21 — Todos os módulos nas Permissões

## Permissões

- A matriz de permissões foi ampliada de 16 para 30 módulos.
- Foram incluídos Clientes e Fornecedores, Stocks, Projetos, Investimentos, RH Financeiro, Contabilidade, Consolidação, Demonstrações Financeiras, Relatórios, Documentos, Auditoria, Centro de Tarefas, Contabilidade Analítica e Administração.
- Todos os botões funcionais do menu lateral passam a respeitar a permissão `Consultar` do respetivo módulo.
- Demonstrações Financeiras também aparece na matriz, mantendo-se visualmente inativo enquanto o módulo não estiver funcional.
- Os módulos novos surgem desmarcados nos perfis existentes e são gravados quando a matriz for guardada.
- O perfil Administrador continua a receber acesso total automaticamente.

## Base de dados

Esta versão não requer migração nem alteração estrutural da base de dados. Os registos de permissão são criados pelo fluxo normal de gravação da matriz.
