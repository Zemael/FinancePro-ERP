# FinancePro v2.1.3 — GitHub Publish Fix

## Correção

- Corrigido o workflow de CI para publicar o projeto WPF com `win-x64` sem reutilizar um `project.assets.json` restaurado sem Runtime Identifier.
- Corrigido o workflow de release com a mesma alteração.
- Mantidos build, testes e validação do Entity Framework.

## Motivo

O comando `dotnet publish` usava `--no-restore` juntamente com `--runtime win-x64`. O restore anterior da solução não havia criado os assets específicos para esse runtime, causando o erro `NETSDK1047`. Agora o próprio `dotnet publish` executa o restore necessário para `win-x64`.

## Base de dados

Nenhuma migration nova é necessária.
