param(
    [string]$Version = "v6.9.3",
    [string]$Destination = (Join-Path (Split-Path -Parent $PSScriptRoot) "FinancePro-$Version.zip")
)

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot
$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("FinancePro-Package-" + [Guid]::NewGuid().ToString("N"))
$packageRoot = Join-Path $tempRoot "FinancePro"

try {
    New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null

    $excludeDirs = @("bin", "obj", ".vs", ".git", "TestResults")
    $excludeFiles = @("*.bak", "*.user", "*.suo", "*.tmp", "*.log")

    $robocopyArgs = @($projectRoot, $packageRoot, "/E", "/NFL", "/NDL", "/NJH", "/NJS", "/NP", "/XD") +
        $excludeDirs + @("/XF") + $excludeFiles

    & robocopy @robocopyArgs | Out-Null
    if ($LASTEXITCODE -ge 8) {
        throw "Robocopy falhou com código $LASTEXITCODE."
    }

    if (Test-Path $Destination) { Remove-Item $Destination -Force }
    Compress-Archive -Path $packageRoot -DestinationPath $Destination -CompressionLevel Optimal

    $zip = Get-Item $Destination
    Write-Host "OK: pacote FinancePro criado." -ForegroundColor Green
    Write-Host ("Ficheiro: {0}" -f $zip.FullName)
    Write-Host ("Tamanho: {0:N2} MB" -f ($zip.Length / 1MB))
    Write-Host "Estrutura: ZIP -> FinancePro -> ficheiros do projeto"
}
finally {
    Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
