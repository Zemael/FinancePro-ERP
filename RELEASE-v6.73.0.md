# FinancePro v6.73.0 — Identidade empresarial nos documentos

O cadastro de empresas passa a permitir selecionar, substituir e remover o logótipo da entidade.

As faturas proforma, faturas definitivas, recibos e notas fiscais utilizam automaticamente o logótipo e os dados da empresa selecionada. O logótipo do FinancePro deixa de aparecer como identidade do emitente.

Quando uma fatura ou recibo estiver integralmente liquidado, a pré-visualização apresenta o carimbo **PAGO**. Documentos pendentes não recebem o carimbo.

A atualização da base é idempotente através do script `055_CompanyLogo.sql` executado pelo Bootstrap.
