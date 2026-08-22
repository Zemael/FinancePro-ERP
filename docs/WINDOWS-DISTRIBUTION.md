# Distribuição para Windows

## Pacote padrão

```powershell
powershell -ExecutionPolicy Bypass -File .\Publicar-Release.ps1
```

O comando valida a solução e gera em `dist`:

- `FinancePro-v6.71.0-win-x64.zip`;
- `FinancePro-v6.71.0-win-x64.zip.sha256`;
- manifesto JSON com o SHA-256 de cada ficheiro publicado.

O pacote padrão é dependente do .NET Desktop Runtime 8 e tem tamanho inferior ao pacote autónomo.

## Pacote autónomo

```powershell
powershell -ExecutionPolicy Bypass -File .\Publicar-Release.ps1 -Autonomo
```

O pacote autónomo inclui o runtime .NET e pode ser maior.

## Outras arquiteturas

```powershell
powershell -ExecutionPolicy Bypass -File .\Publicar-Release.ps1 -Runtime win-x86
powershell -ExecutionPolicy Bypass -File .\Publicar-Release.ps1 -Runtime win-arm64
```
