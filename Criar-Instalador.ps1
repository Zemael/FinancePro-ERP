[CmdletBinding()]
param(
    [switch]$IgnorarValidacao,
    [switch]$DependenteDoRuntime
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
Set-Location $PSScriptRoot

$configuracaoRelease = Get-Content (Join-Path $PSScriptRoot "release.json") -Raw | ConvertFrom-Json
$versao = [string]$configuracaoRelease.versao
$runtime = [string]$configuracaoRelease.runtimePadrao
if ($versao -notmatch '^\d+\.\d+\.\d+$') {
    throw "A versao definida em release.json nao e valida: '$versao'."
}
if ($runtime -ne "win-x64") {
    throw "O instalador suporta atualmente apenas o runtime win-x64."
}
$publicador = Join-Path $PSScriptRoot "Publicar-Release.ps1"
$scriptInno = Join-Path $PSScriptRoot "installer\FinancePro.iss"
$pastaSaida = Join-Path $PSScriptRoot "dist\installer"
$instalador = Join-Path $pastaSaida "FinancePro-Setup-v$versao-win-x64.exe"

$argumentos = @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $publicador, "-Runtime", $runtime)
if (-not $DependenteDoRuntime) { $argumentos += "-Autonomo" }
if ($IgnorarValidacao) { $argumentos += "-IgnorarValidacao" }

Write-Host "== Preparacao do pacote publicado ==" -ForegroundColor Cyan
& powershell @argumentos
if ($LASTEXITCODE -ne 0) {
    throw "A publicacao necessaria ao instalador falhou."
}

$candidatos = @(
    (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
    (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe"),
    (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe")
) | Where-Object { $_ -and (Test-Path $_) }

$compilador = $candidatos | Select-Object -First 1
if (-not $compilador) {
    throw "Inno Setup 6 nao foi encontrado. Instale-o e execute novamente este comando."
}

Write-Host "== Compilacao do instalador ==" -ForegroundColor Cyan
& $compilador "/DMyAppVersion=$versao" "/DPublishedRoot=..\dist\FinancePro-v$versao-$runtime\FinancePro" $scriptInno
if ($LASTEXITCODE -ne 0) {
    throw "A compilacao do instalador falhou com o codigo $LASTEXITCODE."
}

if (-not (Test-Path $instalador)) {
    throw "O instalador esperado nao foi criado: $instalador"
}

$hash = (Get-FileHash -Path $instalador -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($instalador))" | Set-Content -Path "$instalador.sha256" -Encoding ASCII

Write-Host "`nINSTALADOR CRIADO" -ForegroundColor Green
Write-Host "Ficheiro: $instalador"
Write-Host "SHA256: $hash"
