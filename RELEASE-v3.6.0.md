# FinancePro v3.6.0 — Contas a Receber

## Entrega

- nova camada `FinancePro.Application.Receivables`;
- gateway dedicado na camada Data;
- validações centralizadas de títulos e recebimentos;
- resumo de valores em aberto, vencidos e com vencimento hoje;
- pesquisa por código, descrição ou cliente;
- filtro por estado;
- integração preservada com Tesouraria;
- testes unitários do caso de uso;
- nenhuma alteração de esquema ou migration nesta versão.

## Validação necessária

Execute `dotnet build` e `dotnet test` em ambiente com .NET SDK 8.
