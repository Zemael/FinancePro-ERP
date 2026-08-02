# Configuração do GitHub — FinancePro

## 1. Enviar o projeto

```powershell
git init
git branch -M main
git remote add origin https://github.com/Zemael/FinancePro-ERP.git
git add .
git commit -m "chore: establish FinancePro v2 foundation"
git push -u origin main
```

Se o repositório remoto já tiver histórico, faça primeiro `git pull --rebase origin main` e resolva eventuais conflitos antes do push.

## 2. Criar a branch de desenvolvimento

```powershell
git checkout -b develop
git push -u origin develop
```

## 3. Proteger a branch principal

No GitHub: **Settings → Rules → Rulesets → New branch ruleset**.

Aplicar a `main` e ativar:

- Require a pull request before merging;
- Require status checks to pass;
- selecionar o check **Build .NET 8 / WPF** depois da primeira execução;
- Block force pushes;
- Restrict deletions;
- Require conversation resolution.

## 4. Fluxo diário

```powershell
git checkout develop
git pull
git checkout -b feature/nome-da-funcionalidade
# fazer alterações
git add .
git commit -m "feat: descrição objetiva"
git push -u origin feature/nome-da-funcionalidade
```

Depois, abrir um Pull Request para `develop`. Ao fechar uma release, abrir Pull Request de `develop` para `main`.

## 5. Criar uma release automática

```powershell
git checkout main
git pull
git tag v2.0.0
git push origin v2.0.0
```

O workflow `FinancePro Release` compila, publica e anexa o ZIP do Windows à release.
