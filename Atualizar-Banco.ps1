param(
    [string]$NomeMigration = ''
)

$ErrorActionPreference = 'Stop'
$project = '.\FinancePro.Data\FinancePro.Data.csproj'
$startup = '.\FinancePro.UI\FinancePro.UI.csproj'

Get-Process FinancePro.UI -ErrorAction SilentlyContinue | Stop-Process -Force

dotnet build .\FinancePro.sln

if (-not [string]::IsNullOrWhiteSpace($NomeMigration)) {
    dotnet ef migrations add $NomeMigration --project $project --startup-project $startup
}

dotnet ef database update --project $project --startup-project $startup
dotnet ef migrations list --project $project --startup-project $startup
