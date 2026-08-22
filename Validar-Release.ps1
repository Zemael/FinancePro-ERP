[CmdletBinding()]
param(
    [switch]$IgnorarBaseDados,
    [string]$Configuracao = "Release"
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
Set-Location $PSScriptRoot

$inicio = Get-Date
$configuracaoRelease = Get-Content (Join-Path $PSScriptRoot "release.json") -Raw | ConvertFrom-Json
$versaoRelease = [string]$configuracaoRelease.versao
$resultados = [System.Collections.Generic.List[object]]::new()
$pastaArtefactos = Join-Path $PSScriptRoot "artifacts"
$relatorio = Join-Path $pastaArtefactos "release-validation.json"

function Guardar-Relatorio {
    param([string]$Estado)
    if (-not (Test-Path $pastaArtefactos)) {
        New-Item -ItemType Directory -Path $pastaArtefactos | Out-Null
    }

    [ordered]@{
        versao = $versaoRelease
        estado = $Estado
        configuracao = $Configuracao
        inicio = $inicio.ToString("o")
        fim = (Get-Date).ToString("o")
        duracaoSegundos = [math]::Round(((Get-Date) - $inicio).TotalSeconds, 2)
        baseDadosValidada = -not $IgnorarBaseDados
        etapas = $resultados
    } | ConvertTo-Json -Depth 6 | Set-Content -Path $relatorio -Encoding UTF8
}

function Executar-Etapa {
    param(
        [string]$Nome,
        [scriptblock]$Acao
    )

    Write-Host "`n== $Nome ==" -ForegroundColor Cyan
    $cronometro = [System.Diagnostics.Stopwatch]::StartNew()
    & $Acao
    $codigo = $LASTEXITCODE
    $cronometro.Stop()

    $resultados.Add([ordered]@{
        nome = $Nome
        sucesso = ($codigo -eq 0)
        codigoSaida = $codigo
        duracaoSegundos = [math]::Round($cronometro.Elapsed.TotalSeconds, 2)
    })

    if ($codigo -ne 0) {
        Guardar-Relatorio "Falhou"
        throw "A etapa '$Nome' falhou com o codigo $codigo."
    }
}

try {
    Executar-Etapa "Validacao de codificacao" {
        & .\Validar-Codificacao.ps1
    }

    Executar-Etapa "Restore" { dotnet restore .\FinancePro.sln }

    if (-not $IgnorarBaseDados) {
        Executar-Etapa "Bootstrap e validacao da base" {
            dotnet run --project .\FinancePro.Bootstrap --configuration $Configuracao -- --validate
        }
    }

    Executar-Etapa "Build da solucao" {
        dotnet build .\FinancePro.sln --configuration $Configuracao --no-restore
    }

    Executar-Etapa "Testes automatizados" {
        dotnet test .\FinancePro.sln --configuration $Configuracao --no-build
    }

    Guardar-Relatorio "Aprovado"
    Write-Host "`nRELEASE APROVADA" -ForegroundColor Green
    Write-Host "Relatorio: $relatorio"
    exit 0
}
catch {
    if (-not (Test-Path $relatorio)) {
        Guardar-Relatorio "Falhou"
    }
    Write-Error $_
    exit 1
}
