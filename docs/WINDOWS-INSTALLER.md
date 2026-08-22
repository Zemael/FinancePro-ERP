# Instalador Windows do FinancePro

## Pré-requisito de compilação

Instale o Inno Setup 6 no computador usado para gerar o instalador.

## Gerar o instalador

```powershell
powershell -ExecutionPolicy Bypass -File .\Gerar-Release.ps1
```

Por padrão, o instalador inclui o runtime .NET 8. O resultado será criado em:

```text
dist\installer\FinancePro-Setup-v6.71.0-win-x64.exe
```

Também será criado o respetivo ficheiro `.sha256`.

## Funcionalidades

- Instalação em `Program Files\FinancePro`.
- Atalho no Menu Iniciar.
- Atalho opcional no Ambiente de Trabalho.
- Entrada própria para desinstalação no Windows.
- Atualização sobre versões futuras através de um `AppId` permanente.
- Preservação de `appsettings.json` durante atualizações e desinstalação.

## Pacote dependente do runtime

Para gerar um instalador menor que exige .NET Desktop Runtime 8 instalado:

```powershell
powershell -ExecutionPolicy Bypass -File .\Gerar-Release.ps1 -DependenteDoRuntime
```
