# Arquitetura

## Camadas atuais

- **FinancePro.Core**: entidades, enums, DTOs e contratos existentes.
- **FinancePro.Application**: contratos transversais e, progressivamente, casos de uso. Não referencia WPF nem Entity Framework.
- **FinancePro.Data**: DbContext, configurações EF Core, repositórios e serviços legados de acesso a dados.
- **FinancePro.UI**: WPF, Views, ViewModels e implementações de serviços dependentes da interface.

## Dependências permitidas

```text
UI -> Application -> Core
UI -> Data -> Core
Application -> Core
```

A migração dos serviços de negócio de `Data/Services` para casos de uso em `Application` será incremental, sem reescrever módulos estáveis de uma só vez.

## Composition root

`FinancePro.UI/App.xaml.cs` é o único ponto responsável por montar o contentor de injeção de dependências.
