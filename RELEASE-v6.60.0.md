# FinancePro v6.60.0 — Gate Automatizado de Produção

- Novo comando único `Validar-Release.ps1`.
- Restore, Bootstrap, build e testes executados em sequência controlada.
- Interrupção imediata e código de saída diferente de zero quando uma etapa falha.
- Relatório JSON com estado, duração e código de cada etapa.
- Opção `-IgnorarBaseDados` para máquinas sem SQL Server.
- Documentação formal dos critérios de preparação para produção.
- Nenhuma alteração à base de dados.

## Execução

```powershell
.\Validar-Release.ps1
```
