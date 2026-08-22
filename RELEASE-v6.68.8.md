# FinancePro v6.68.8 — Correção da pré-visualização WPF

Correção pontual do conflito de namespace encontrado durante a compilação Windows da v6.68.7.

`System.Windows.Application.Current` passa a ser utilizado explicitamente ao associar a janela de pré-visualização à janela principal do FinancePro.
