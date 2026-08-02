# Estratégia de Branches

- `main`: releases estáveis.
- `develop`: integração das funcionalidades aprovadas.
- `feature/*`: novas funcionalidades.
- `fix/*`: correções normais.
- `hotfix/*`: correções urgentes derivadas de `main`.
- `release/*`: preparação de uma release.

Fluxo atual da Foundation:

`feature/foundation` → Pull Request → `develop` → Pull Request de release → `main`.
