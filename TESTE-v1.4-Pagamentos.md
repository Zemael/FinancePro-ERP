# FinancePro ERP v1.4 — Pagamentos

## Objetivo
Validar o fluxo completo de conta a pagar e pagamento pela Tesouraria.

## Cenários

1. Abra **Despesas** e crie uma conta a pagar com descrição, valor e vencimento.
2. Selecione uma caixa com sessão aberta ou uma conta bancária ativa.
3. Escolha a data do pagamento e clique em **Pagar**.
4. Confirme que a conta passa para **Paga** e que surge um movimento de saída na Tesouraria.
5. Tente pagar por uma caixa sem sessão aberta: o sistema deve bloquear.
6. Tente pagar uma conta já paga: o sistema deve bloquear.
7. Tente usar data anterior à emissão: o sistema deve bloquear.
8. Numa caixa que não permite saldo negativo, tente pagar valor superior ao saldo: o sistema deve bloquear.

## Banco de dados
Esta versão não cria novas tabelas nem colunas. Não é necessária uma nova migration se a base da v1.2 já estiver atualizada.

## Build
Feche o FinancePro antes de compilar para evitar ficheiros DLL bloqueados.

```powershell
dotnet clean .\FinancePro.sln
dotnet build .\FinancePro.sln
dotnet run --project .\FinancePro.UI\FinancePro.UI.csproj
```
