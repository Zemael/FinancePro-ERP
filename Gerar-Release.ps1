[CmdletBinding()]
param(
    [switch]$DependenteDoRuntime,
    [switch]$IgnorarValidacao
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
Set-Location $PSScriptRoot

$inicio = Get-Date
$configuracao = Get-Content (Join-Path $PSScriptRoot "release.json") -Raw | ConvertFrom-Json
$versao = [string]$configuracao.versao
$runtime = [string]$configuracao.runtimePadrao
$pastaDist = Join-Path $PSScriptRoot "dist"
$zip = Join-Path $pastaDist "FinancePro-v$versao-$runtime.zip"
$instalador = Join-Path $pastaDist "installer\FinancePro-Setup-v$versao-$runtime.exe"
$relatorio = Join-Path $pastaDist "release-v$versao.json"

if ($versao -notmatch '^\d+\.\d+\.\d+$') {
    throw "A versao definida em release.json nao e valida: '$versao'."
}

$argumentos = @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", (Join-Path $PSScriptRoot "Criar-Instalador.ps1"))
if ($DependenteDoRuntime) { $argumentos += "-DependenteDoRuntime" }
if ($IgnorarValidacao) { $argumentos += "-IgnorarValidacao" }

try {
    Write-Host "== Release completa FinancePro v$versao ==" -ForegroundColor Cyan
    & powershell @argumentos
    if ($LASTEXITCODE -ne 0) {
        throw "A criacao da release falhou com o codigo $LASTEXITCODE."
    }

    foreach ($ficheiro in @($zip, "$zip.sha256", $instalador, "$instalador.sha256")) {
        if (-not (Test-Path $ficheiro)) {
            throw "Artefacto obrigatorio nao encontrado: $ficheiro"
        }
    }

    $resultado = [ordered]@{
        produto = "FinancePro"
        versao = $versao
        runtime = $runtime
        autonomo = -not $DependenteDoRuntime
        estado = "Aprovado"
        criadoEm = (Get-Date).ToString("o")
        duracaoSegundos = [math]::Round(((Get-Date) - $inicio).TotalSeconds, 2)
        artefactos = @(
            [ordered]@{ tipo = "zip"; caminho = $zip; sha256 = (Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant() }
            [ordered]@{ tipo = "instalador"; caminho = $instalador; sha256 = (Get-FileHash $instalador -Algorithm SHA256).Hash.ToLowerInvariant() }
        )
    }
    $resultado | ConvertTo-Json -Depth 5 | Set-Content -Path $relatorio -Encoding UTF8

    Write-Host "`nRELEASE COMPLETA APROVADA" -ForegroundColor Green
    Write-Host "ZIP: $zip"
    Write-Host "Instalador: $instalador"
    Write-Host "Relatorio: $relatorio"
}
catch {
    [ordered]@{
        produto = "FinancePro"
        versao = $versao
        runtime = $runtime
        estado = "Falhou"
        criadoEm = (Get-Date).ToString("o")
        duracaoSegundos = [math]::Round(((Get-Date) - $inicio).TotalSeconds, 2)
        erro = $_.Exception.Message
    } | ConvertTo-Json -Depth 4 | Set-Content -Path $relatorio -Encoding UTF8
    throw
}
