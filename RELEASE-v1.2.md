# FinancePro ERP v1.2 — Tesouraria Base

## Correções consolidadas

- Corrigido o cálculo do saldo da sessão de caixa.
- O saldo inicial deixa de ser contado duas vezes: o movimento de abertura já representa esse valor.
- Mantidas as correções de XAML, recursos e ViewModel da versão 1.1.4.
- Adicionado `Build-Limpo.ps1` para encerrar a aplicação, remover `bin/obj`, restaurar e compilar.
- Adicionado `Atualizar-Banco.ps1` para compilar, criar migration opcional e aplicar a base de dados.

## Build recomendado

Feche a execução no Visual Studio ou use:

```powershell
.\Build-Limpo.ps1
```

## Atualizar banco sem criar migration

```powershell
.\Atualizar-Banco.ps1
```

## Criar e aplicar uma nova migration

```powershell
.\Atualizar-Banco.ps1 -NomeMigration CriarSessoesCaixa
```

## Teste funcional

1. Abrir o FinancePro.
2. Entrar com um utilizador ativo.
3. Abrir `Tesouraria > Caixa`.
4. Criar ou selecionar um caixa ativo.
5. Abrir com saldo inicial de teste, por exemplo `100000`.
6. Confirmar que o saldo atual é `100 000 FCFA`, e não `200 000 FCFA`.
7. Fechar o caixa usando o mesmo saldo contado.
8. Confirmar diferença igual a zero.
