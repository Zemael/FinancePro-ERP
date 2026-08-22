# FinancePro v6.42.0 — Planeamento e Cadastro de Investimentos

- Cadastro completo com código, nome, tipo, departamento e gestor.
- Datas, orçamento, valor executado, retorno esperado, estado e observações.
- Edição, aprovação e arquivo lógico de investimentos.
- Validações obrigatórias, financeiras e cronológicas.
- Nova tabela idempotente `Investments` pelo Bootstrap.
- Requer execução do Bootstrap para atualização do esquema; não usa migration EF.
