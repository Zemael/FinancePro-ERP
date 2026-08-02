# CI/CD

O workflow `FinancePro CI` é executado em pushes e Pull Requests para `main` e `develop`.

Etapas:

1. Checkout;
2. instalação do .NET 8;
3. restore;
4. build Release;
5. testes unitários;
6. listagem das migrations;
7. verificação de alterações pendentes no modelo;
8. publicação WPF para `win-x64`;
9. upload do artefacto.

Tags `v*.*.*` acionam o workflow de release, que publica o executável e cria uma GitHub Release.
