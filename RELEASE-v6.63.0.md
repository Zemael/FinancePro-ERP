# FinancePro v6.63.0 — Pipeline Mestre de Release

- Versão centralizada no ficheiro `release.json`.
- Número de versão sincronizado entre assemblies, pacote ZIP e instalador.
- Novo comando único `Gerar-Release.ps1` para validação, publicação e instalador.
- Verificação obrigatória de todos os artefactos e respetivos checksums SHA-256.
- Relatório final estruturado em `dist\release-v6.63.0.json`.
- Nenhuma alteração à base de dados.

## Execução

```powershell
powershell -ExecutionPolicy Bypass -File .\Gerar-Release.ps1
```
