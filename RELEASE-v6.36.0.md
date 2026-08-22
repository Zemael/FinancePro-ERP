# FinancePro v6.36.0 — Retenção e Limpeza Controlada de Backups

- Retenção configurável entre 1 e 100 backups recentes.
- Limpeza restrita aos ficheiros `FinancePro_*.bak` criados pela aplicação.
- Exclusão apenas na pasta padrão de backups do SQL Server.
- Ficheiros de ligação/reparse points são ignorados por segurança.
- Confirmação explícita obrigatória antes da eliminação.
- Resumo da quantidade eliminada, preservada e espaço libertado.
- Execução assíncrona e atualização automática do histórico.
- Sem nova tabela e sem migration.
