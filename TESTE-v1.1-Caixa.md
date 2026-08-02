# FinancePro ERP v1.1 — Abertura e Fecho de Caixa

## Atualizar a base de dados
Na raiz da solução:

```powershell
dotnet ef migrations add CriarSessoesCaixa --project .\FinancePro.Data\FinancePro.Data.csproj --startup-project .\FinancePro.UI\FinancePro.UI.csproj
dotnet ef database update --project .\FinancePro.Data\FinancePro.Data.csproj --startup-project .\FinancePro.UI\FinancePro.UI.csproj
```

## Teste funcional
1. Entre como Administrador.
2. Abra **Caixa** no menu.
3. Crie uma caixa ativa, caso ainda não exista.
4. Selecione a caixa.
5. Indique o saldo inicial e clique em **Abrir Caixa**.
6. Confirme que o cartão superior mostra estado **Aberto**, operador, data e saldo.
7. Tente abrir a mesma caixa novamente: a operação deve ser bloqueada.
8. Indique o saldo contado e clique em **Fechar Caixa**.
9. Confirme que o estado muda para **Fechado**.

## Regras incluídas
- Não abre caixa inativo.
- Não permite saldo inicial negativo.
- Não permite duas sessões abertas para o mesmo caixa.
- Não desativa caixa com sessão aberta.
- Exige exercício financeiro aberto.
- Abertura com valor positivo gera movimento de entrada.
