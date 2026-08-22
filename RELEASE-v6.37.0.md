# FinancePro v6.37.0 — Verificação de Integridade do Backup

- Localização automática do último backup completo da base ativa.
- Verificação nativa com `RESTORE VERIFYONLY` e validação de checksum.
- Operação não destrutiva: nenhuma base é restaurada ou substituída.
- Resultado com data do backup e momento da verificação.
- Timeout operacional de cinco minutos.
- Painel integrado no Histórico de Backups em Configurações.
- Sem nova tabela e sem migration.
