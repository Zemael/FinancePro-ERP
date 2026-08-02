# Coding Standards

- C# com nullable reference types ativados.
- Classes e membros públicos em PascalCase; variáveis locais em camelCase.
- Métodos assíncronos terminam em `Async`.
- Uma classe principal por ficheiro.
- A UI não deve criar ou consultar diretamente o DbContext.
- Casos de uso retornam `Result` ou `Result<T>` quando houver falha esperada de negócio.
- Não ocultar exceções inesperadas; registá-las e apresentar mensagem apropriada.
- Toda alteração de modelo exige migration EF Core própria, exceto quando o modelo não mudou.
