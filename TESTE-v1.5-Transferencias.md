# Teste — v1.5 Transferências

1. Feche o FinancePro antes de compilar.
2. Execute `dotnet clean .\FinancePro.sln`.
3. Execute `dotnet build .\FinancePro.sln`.
4. Abra o FinancePro e entre em **Tesouraria**.
5. Selecione **Transferência**.
6. Escolha origem e destino diferentes.
7. Informe descrição e valor maior que zero.
8. Registe a operação.
9. Confirme na grelha dois movimentos com o mesmo valor: uma saída e uma entrada.

## Cenários obrigatórios
- Origem e destino iguais: deve bloquear.
- Caixa de origem sem sessão aberta: deve bloquear.
- Caixa sem saldo suficiente e sem permissão de saldo negativo: deve bloquear.
- Caixa ou conta inativa: deve bloquear.
- Transferência válida: deve gerar os dois movimentos sem gravação parcial.
