$ErrorActionPreference = "Stop"; Set-Location $PSScriptRoot
dotnet ef dbcontext info --project .\FinancePro.Data\FinancePro.Data.csproj --startup-project .\FinancePro.Data\FinancePro.Data.csproj --configuration Release
if ($LASTEXITCODE) { exit $LASTEXITCODE }
dotnet ef migrations has-pending-model-changes --project .\FinancePro.Data\FinancePro.Data.csproj --startup-project .\FinancePro.Data\FinancePro.Data.csproj --configuration Release
if ($LASTEXITCODE) { exit $LASTEXITCODE }
dotnet run --project .\FinancePro.Bootstrap\FinancePro.Bootstrap.csproj --configuration Release -- --validate
exit $LASTEXITCODE

