# Gate de preparação para produção

Uma versão só é considerada aprovada quando todas as etapas abaixo terminarem com código de saída zero:

1. Restore completo da solução.
2. Bootstrap idempotente e validação das tabelas essenciais.
3. Build `Release` de todos os projetos.
4. Execução integral dos testes automatizados.

## Execução recomendada

```powershell
.\Validar-Release.ps1
```

Para validar apenas o código numa máquina sem SQL Server:

```powershell
.\Validar-Release.ps1 -IgnorarBaseDados
```

O resultado é gravado em `artifacts\release-validation.json`, incluindo estado, duração e código de saída de cada etapa.

## Critérios atuais

- 18 de 18 módulos funcionais concluídos.
- Build `Release` sem erros.
- 74 testes automatizados esperados.
- Base de dados acessível e scripts idempotentes aplicados.
- Nenhum `bin`, `obj`, `.vs` ou `.git` incluído no pacote de distribuição.
