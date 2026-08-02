# Regras de Negócio — Estado Atual

## Segurança
- Utilizador autenticado possui empresa, perfil e permissões de sessão.
- O perfil Administrador possui acesso total.

## Caixa
- Caixa inativa não pode ser aberta.
- Não é permitido saldo inicial negativo.
- Um caixa não pode possuir duas sessões abertas simultaneamente.
- Operações em caixa exigem sessão aberta quando definido pelo fluxo.

## Transferências
- Origem e destino devem ser diferentes.
- A operação deve ser gravada de forma transacional.
- A origem deve ter saldo suficiente quando saldo negativo não for permitido.
