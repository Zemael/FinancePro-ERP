# FinancePro v2.7.1 — Estabilização da ligação SQL Server

## Correções

- Aumentado o tempo limite de ligação e de comandos para 60 segundos.
- Ativada a política de repetição automática do Entity Framework Core para falhas transitórias.
- Limpeza do pool de ligações antes da primeira consulta da aplicação.
- Verificação explícita de disponibilidade da base de dados no arranque.
- Mensagem amigável com opção de tentar novamente quando o SQL Server estiver indisponível.
- Cancelamento controlado do arranque, evitando exceções não tratadas no WPF.
- Consulta inicial de utilizadores executada sem tracking e com token de cancelamento.

## Base de dados

Nenhuma nova migration é necessária.
