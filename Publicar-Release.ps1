[CmdletBinding()]
param(
    [string]$Runtime = "win-x64",
    [switch]$Autonomo,
    [switch]$IgnorarValidacao
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
Set-Location $PSScriptRoot

$configuracaoRelease = Get-Content (Join-Path $PSScriptRoot "release.json") -Raw | ConvertFrom-Json
$versao = [string]$configuracaoRelease.versao
if ($versao -notmatch '^\d+\.\d+\.\d+$') {
    throw "A versao definida em release.json nao e valida: '$versao'."
}
$nomePacote = "FinancePro-v$versao-$Runtime"
$pastaDist = Join-Path $PSScriptRoot "dist"
$pastaPacote = Join-Path $pastaDist $nomePacote
$pastaAplicacao = Join-Path $pastaPacote "FinancePro"
$ficheiroZip = Join-Path $pastaDist "$nomePacote.zip"
$ficheiroChecksum = "$ficheiroZip.sha256"
$ficheiroManifesto = Join-Path $pastaAplicacao "manifest.json"

if (-not $IgnorarValidacao) {
    Write-Host "== Gate de validacao ==" -ForegroundColor Cyan
    & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot "Validar-Release.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "A publicacao foi interrompida porque o gate de validacao falhou."
    }
}

if (Test-Path $pastaPacote) {
    Remove-Item -Path $pastaPacote -Recurse -Force
}
if (Test-Path $ficheiroZip) {
    Remove-Item -Path $ficheiroZip -Force
}
if (Test-Path $ficheiroChecksum) {
    Remove-Item -Path $ficheiroChecksum -Force
}

New-Item -ItemType Directory -Path $pastaAplicacao -Force | Out-Null
$selfContained = if ($Autonomo) { "true" } else { "false" }

Write-Host "== Publicacao $Runtime ==" -ForegroundColor Cyan
dotnet restore .\FinancePro.UI\FinancePro.UI.csproj --runtime $Runtime
if ($LASTEXITCODE -ne 0) {
    throw "O restore especifico do runtime falhou com o codigo $LASTEXITCODE."
}
dotnet publish .\FinancePro.UI\FinancePro.UI.csproj `
    --configuration Release `
    --runtime $Runtime `
    --self-contained $selfContained `
    --output $pastaAplicacao `
    --no-restore `
    /p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish falhou com o codigo $LASTEXITCODE."
}

$ficheiros = Get-ChildItem -Path $pastaAplicacao -File -Recurse | ForEach-Object {
    [ordered]@{
        caminho = $_.FullName.Substring($pastaAplicacao.Length + 1)
        tamanhoBytes = $_.Length
        sha256 = (Get-FileHash -Path $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    }
}

[ordered]@{
    produto = "FinancePro"
    versao = $versao
    runtime = $Runtime
    autonomo = [bool]$Autonomo
    criadoEm = (Get-Date).ToString("o")
    executavel = "FinancePro.UI.exe"
    totalFicheiros = @($ficheiros).Count
    ficheiros = $ficheiros
} | ConvertTo-Json -Depth 6 | Set-Content -Path $ficheiroManifesto -Encoding UTF8

@"
FINANCEPRO v$versao

1. Confirme que o SQL Server Express e a base FinancePro estao disponiveis.
2. Abra a pasta FinancePro.
3. Execute FinancePro.UI.exe.
4. Este pacote foi gerado para $Runtime.
5. Autonomo: $([bool]$Autonomo).
"@ | Set-Content -Path (Join-Path $pastaAplicacao "LEIA-ME.txt") -Encoding UTF8

Write-Host "== Compactacao e checksum ==" -ForegroundColor Cyan
Compress-Archive -Path $pastaAplicacao -DestinationPath $ficheiroZip -CompressionLevel Optimal
$hash = (Get-FileHash -Path $ficheiroZip -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($ficheiroZip))" | Set-Content -Path $ficheiroChecksum -Encoding ASCII

Write-Host "`nPACOTE CRIADO" -ForegroundColor Green
Write-Host "ZIP: $ficheiroZip"
Write-Host "SHA256: $hash"
