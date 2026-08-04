# FinancePro v5.4.0 — Document Engine Foundation

## Entregue

- Motor documental na camada `FinancePro.Platform`.
- Catálogo inicial de modelos institucionais.
- Informação para despacho, ofício, memorando, parecer técnico, auto de transferência patrimonial e guia de remessa.
- Extração automática dos campos de cada modelo.
- Pré-visualização do documento gerado.
- Exportação em TXT e HTML imprimível.
- Novo módulo **Documentos** na navegação principal.
- Testes automatizados de geração e campos.

## Base técnica

Esta fundação não altera o modelo EF Core e não requer migration. Os modelos estão centralizados no serviço documental e poderão ser migrados para persistência configurável numa versão posterior.
