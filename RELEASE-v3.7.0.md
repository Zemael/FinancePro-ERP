# FinancePro v3.7.0 — Contas a Pagar

## Incluído

- camada de aplicação `FinancePro.Application.Payables`;
- gateway dedicado `FinancePro.Data.Payables`;
- carregamento consolidado de títulos, fornecedores, categorias e origens de pagamento;
- validações centralizadas para criação, pagamento e cancelamento;
- pesquisa e filtros disponíveis no serviço de aplicação;
- indicadores de total em aberto, vencido, a vencer hoje e pago;
- `DespesasViewModel` migrado para a camada Application;
- integração com Tesouraria preservada;
- testes unitários do módulo;
- nenhuma alteração ao modelo de dados e nenhuma migration necessária.

## Limite desta versão

O modelo atual liquida o título integralmente. Pagamentos parciais e estornos financeiros exigem histórico de parcelas/movimentos e serão introduzidos numa migration própria.
