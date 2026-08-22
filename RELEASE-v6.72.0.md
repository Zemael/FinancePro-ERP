# FinancePro v6.72.0 — Editor de múltiplos itens

O editor de faturação passa a apresentar campos identificados para produto ou serviço, quantidade, preço unitário, desconto e IVA.

A tabela de linhas inclui código, descrição, unidade, quantidade, preço, base tributável, IVA e total, além da contagem de itens do documento.

As validações impedem quantidades inválidas e percentagens fora do intervalo de 0% a 100%. A inclusão, edição e remoção de cada linha, juntamente com o recálculo do total da fatura, são executadas numa única transação resiliente.
