$ErrorActionPreference = 'Stop'

Write-Host 'A encerrar instâncias do FinancePro...' -ForegroundColor Cyan
Get-Process FinancePro.UI -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500

Write-Host 'A limpar ficheiros temporários...' -ForegroundColor Cyan
Get-ChildItem -Path . -Directory -Recurse -Force |
    Where-Object { $_.Name -in @('bin', 'obj') } |
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host 'A restaurar pacotes...' -ForegroundColor Cyan
dotnet restore .\FinancePro.sln

Write-Host 'A compilar solução...' -ForegroundColor Cyan
dotnet build .\FinancePro.sln --no-restore

Write-Host 'Build concluído.' -ForegroundColor Green
