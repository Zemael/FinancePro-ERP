# FinancePro v6.65.20 — Recorte dinâmico dos cabeçalhos

## Ajuste visual

- Adicionado o componente reutilizável `RoundedClipBorder` para recortar o cabeçalho completo.
- A geometria do recorte é recalculada em cada alteração de layout da tabela.
- O canto superior direito permanece arredondado durante e depois da expansão ou contração do menu lateral.
- O recorte acompanha a largura real da área visível e não depende da última coluna.
- Preservados os cabeçalhos sem linhas verticais e os cantos inferiores retos.
- Nenhuma coluna, binding, paginação ou funcionalidade foi alterada.

## Base de dados

Esta versão não requer migração nem alteração da base de dados.
