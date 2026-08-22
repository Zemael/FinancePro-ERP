#requires -Version 5.1
<#
  Auditoria-Visual.ps1
  Verifica o XAML do FinancePro.UI contra os padrões definidos em
  docs/CODING-STANDARDS.md (secção "Padrões visuais"), para apanhar antes do
  build o tipo de inconsistência que antes só se via ecrã a ecrã:
    - cores fixas em vez de DynamicResource
    - botões com Width fixo
    - colunas de tabela com Width fixo (não Auto/*)
    - tabelas que não usam o padrão visual FP.DataGrid
    - propriedades visuais locais que anulam o padrão das tabelas
    - estilos locais que duplicam os globais (Cartao/Rotulo/BotaoPrimario/...)
    - chaves StaticResource/DynamicResource sem definição em lado nenhum
    - artefactos de encoding (mojibake tipo "Ã§", "Â·")

  Uso:
    .\Auditoria-Visual.ps1              # relatório normal
    .\Auditoria-Visual.ps1 -Detalhado   # lista cada ocorrência, não só a contagem
#>

param(
    [switch]$Detalhado
)

$ErrorActionPreference = "Stop"
$raiz = Join-Path $PSScriptRoot "FinancePro.UI"
$views = Get-ChildItem -Path (Join-Path $raiz "Views") -Filter *.xaml -Recurse
$resources = Get-ChildItem -Path (Join-Path $raiz "Resources") -Filter *.xaml -Recurse
$mainWindow = Join-Path $raiz "MainWindow.xaml"
$todosXaml = @($views) + @($resources) + (Get-Item $mainWindow)

# Ecrãs com exceção deliberada a cores fixas (marca própria, não seguem o tema)
$excecoesCor = @("LoginView.xaml", "SetupView.xaml", "MainWindow.xaml")

$problemas = New-Object System.Collections.Generic.List[Object]

function Registar($Categoria, $Ficheiro, $Linha, $Excerto) {
    $problemas.Add([PSCustomObject]@{
        Categoria = $Categoria
        Ficheiro  = $Ficheiro
        Linha     = $Linha
        Excerto   = $Excerto.Trim().Substring(0, [Math]::Min(120, $Excerto.Trim().Length))
    })
}

Write-Host "=== Auditoria Visual FinancePro ===" -ForegroundColor Cyan
Write-Host "A verificar $($todosXaml.Count) ficheiros XAML...`n"

# --- 1. Cores fixas fora das exceções -----------------------------------------
foreach ($f in $views) {
    if ($excecoesCor -contains $f.Name) { continue }
    $linhas = Get-Content $f.FullName
    for ($i = 0; $i -lt $linhas.Count; $i++) {
        if ($linhas[$i] -match '(Background|Foreground|BorderBrush|Fill|Stroke)="#[0-9A-Fa-f]{3,8}"') {
            Registar "Cor fixa" $f.Name ($i + 1) $linhas[$i]
        }
    }
}

# --- 2. Botões com Width fixo ---------------------------------------------------
foreach ($f in $todosXaml) {
    $conteudo = Get-Content $f.FullName -Raw
    $matches = [regex]::Matches($conteudo, '<Button\b[^>]*?\sWidth="[0-9]+(\.[0-9]+)?"[^>]*/?>')
    foreach ($m in $matches) {
        Registar "Botão com Width fixo" $f.Name "-" $m.Value
    }
}

# --- 3. Colunas de tabela com Width fixo ----------------------------------------
foreach ($f in $views) {
    $conteudo = Get-Content $f.FullName -Raw
    $matches = [regex]::Matches($conteudo, '<DataGrid(Text|CheckBox)Column\b[^>]*?\sWidth="[0-9]+(\.[0-9]+)?"[^>]*/?>')
    foreach ($m in $matches) {
        Registar "Coluna com Width fixo" $f.Name "-" $m.Value
    }
}

# --- 4. Padrão global das tabelas ------------------------------------------------
foreach ($f in $views) {
    $conteudo = Get-Content $f.FullName -Raw
    $matches = [regex]::Matches($conteudo, '<DataGrid(?=\s|>)[^>]*>')
    foreach ($m in $matches) {
        if ($m.Value -notmatch 'Style="\{StaticResource FP\.DataGrid\}"') {
            Registar "Tabela fora do padrão" $f.Name "-" $m.Value
        }
        if ($m.Value -match '\s(RowHeight|MinRowHeight|HeadersVisibility|GridLinesVisibility|BorderThickness)="') {
            Registar "Tabela com visual local" $f.Name "-" $m.Value
        }
    }
}

# --- 5. Estilos locais que duplicam os globais ----------------------------------
$chavesGlobais = @("Cartao", "Card", "Rotulo", "BotaoPrimario", "BotaoSecundario", "TituloModulo", "TextoSecundario")
foreach ($f in $views) {
    $conteudo = Get-Content $f.FullName -Raw
    foreach ($chave in $chavesGlobais) {
        if ($conteudo -match "<Style x:Key=`"$chave`"") {
            Registar "Estilo local duplicado" $f.Name "-" "x:Key=`"$chave`" redefinido localmente"
        }
    }
}

# --- 6. Chaves StaticResource/DynamicResource sem definição --------------------
$usadas = New-Object System.Collections.Generic.HashSet[string]
$definidas = New-Object System.Collections.Generic.HashSet[string]
foreach ($f in $todosXaml) {
    $conteudo = Get-Content $f.FullName -Raw
    # só dentro de "{...Resource ...}" (evita apanhar texto de comentários XML)
    foreach ($m in [regex]::Matches($conteudo, '\{(Static|Dynamic)Resource\s+([A-Za-z0-9_.]+)\}')) {
        [void]$usadas.Add($m.Groups[2].Value)
    }
    foreach ($m in [regex]::Matches($conteudo, 'x:Key="([A-Za-z0-9_. ]+)"')) {
        [void]$definidas.Add($m.Groups[1].Value)
    }
}
# Conversores definidos em App.xaml também contam
$appXaml = Join-Path $raiz "App.xaml"
if (Test-Path $appXaml) {
    foreach ($m in [regex]::Matches((Get-Content $appXaml -Raw), 'x:Key="([A-Za-z0-9_. ]+)"')) {
        [void]$definidas.Add($m.Groups[1].Value)
    }
}
foreach ($chave in $usadas) {
    if (-not $definidas.Contains($chave)) {
        Registar "Chave de recurso órfã" "(vário)" "-" $chave
    }
}

# --- 7. Artefactos de encoding (mojibake) ---------------------------------------
# Sequências específicas de dupla-codificação UTF-8→Windows-1252. Não usar "Ã."
# genérico: maiúsculas legítimas em PT (ex.: "AQUISIÇÃO", "EXECUÇÃO") também
# começam por "Ã" e dariam falso positivo.
$padraoMojibake = 'Ã©|Ã§|Ã£|Ã³|Ã¡|Ãª|Ã­|Ãµ|Ã¢|Âº|Â·|â€"|â€œ|â€\x9d'
foreach ($f in (Get-ChildItem -Path $raiz -Include *.cs, *.xaml -Recurse)) {
    $conteudo = Get-Content $f.FullName -Raw -ErrorAction SilentlyContinue
    if ($null -ne $conteudo -and $conteudo -match $padraoMojibake) {
        Registar "Possível mojibake" $f.Name "-" "contém sequências de dupla-codificação — rever encoding"
    }
}

# --- Relatório -------------------------------------------------------------------
if ($problemas.Count -eq 0) {
    Write-Host "Nenhum problema encontrado. Tudo em conformidade com docs/CODING-STANDARDS.md." -ForegroundColor Green
    exit 0
}

$resumo = $problemas | Group-Object Categoria | Sort-Object Count -Descending
Write-Host "Encontrados $($problemas.Count) ponto(s) a rever:`n" -ForegroundColor Yellow
foreach ($grupo in $resumo) {
    Write-Host ("  {0,-28} {1,3}" -f $grupo.Name, $grupo.Count) -ForegroundColor Yellow
}

if ($Detalhado) {
    Write-Host "`n--- Detalhe ---`n"
    $problemas | Format-Table Categoria, Ficheiro, Linha, Excerto -AutoSize -Wrap
} else {
    Write-Host "`n(usa -Detalhado para veres cada ocorrência)"
}

exit 1
