#requires -Version 5.1
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
Set-Location $PSScriptRoot

$extensoes = @(
    ".cs", ".xaml", ".csproj", ".props", ".targets", ".json", ".xml",
    ".config", ".ps1", ".iss", ".md", ".txt", ".sql", ".csv"
)
$ignorados = @("Auditoria-Visual.ps1", "Validar-Codificacao.ps1")

# Assinaturas comuns de UTF-8 interpretado como Windows-1252, dupla codificacao
# e caracteres de substituicao. O padrao evita procurar apenas por "Ã", porque
# esse caractere aparece legitimamente em palavras portuguesas em maiusculas.
$padrao = 'Â(?=[\s·ºª.;,:!?])|Ã(?:©|§|£|³|¡|ª|­|µ|¢|º|¼|¶|‰|š|œ)|â(?:€|€¢|„¢|†|œ|˜)|ï»¿|ðŸ|�'
$problemas = [System.Collections.Generic.List[object]]::new()

$ficheiros = Get-ChildItem -Path $PSScriptRoot -File -Recurse | Where-Object {
    $extensoes -contains $_.Extension.ToLowerInvariant() -and
    $ignorados -notcontains $_.Name -and
    $_.FullName -notmatch '[\\/](bin|obj|artifacts|dist)[\\/]'
}

foreach ($ficheiro in $ficheiros) {
    $numeroLinha = 0
    foreach ($linha in (Get-Content -LiteralPath $ficheiro.FullName -Encoding UTF8)) {
        $numeroLinha++
        if ($linha -match $padrao) {
            $problemas.Add([pscustomobject]@{
                Ficheiro = $ficheiro.FullName.Substring($PSScriptRoot.Length + 1)
                Linha = $numeroLinha
                Excerto = $linha.Trim()
            })
        }
    }
}

if ($problemas.Count -gt 0) {
    Write-Host "Foram encontrados $($problemas.Count) possiveis artefactos de codificacao:" -ForegroundColor Red
    $problemas | Format-Table Ficheiro, Linha, Excerto -AutoSize -Wrap
    exit 1
}

Write-Host "OK: codificacao textual validada em $($ficheiros.Count) ficheiros." -ForegroundColor Green
exit 0
