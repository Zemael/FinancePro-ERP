# Contribuir para o FinancePro ERP

O FinancePro é um projeto proprietário. Contribuições exigem autorização do titular.

## Fluxo de trabalho

1. Atualize `develop`.
2. Crie uma branch `feature/*`, `fix/*`, `docs/*` ou `chore/*`.
3. Faça commits pequenos usando Conventional Commits.
4. Execute `dotnet build FinancePro.sln` e `dotnet test FinancePro.sln`.
5. Abra Pull Request para `develop`.

## Requisitos do Pull Request

- Build e testes verdes;
- migrations verificadas quando o modelo mudar;
- documentação atualizada;
- ausência de segredos, `bin`, `obj` e ficheiros locais;
- descrição do impacto e dos testes realizados.
