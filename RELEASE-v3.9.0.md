# FinancePro v3.9.0 — Platform Services

## Incluído
- Financial Engine transversal para orquestrar Tesouraria, Contabilidade, auditoria e eventos.
- Serviço central de numeração por empresa, módulo e ano.
- Dispatcher simples de eventos de domínio.
- Auditoria transversal sobre a tabela existente `LogsAuditoria`.
- Script `012_PlatformServices.sql` para `SequenciasDocumentos`.
- Testes do serviço de numeração.

## Base de dados
Execute `FinancePro.Data/Scripts/Schema/012_PlatformServices.sql` antes de utilizar a numeração automática.
A tabela é acedida por SQL transacional e não altera o modelo EF Core nesta entrega.
