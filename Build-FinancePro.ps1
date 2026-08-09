$ErrorActionPreference = "Stop"; Set-Location $PSScriptRoot
dotnet restore .\FinancePro.sln; if ($LASTEXITCODE) { exit $LASTEXITCODE }
dotnet ef database update --project .\FinancePro.Data\FinancePro.Data.csproj --startup-project .\FinancePro.UI\FinancePro.UI.csproj --configuration Release; if ($LASTEXITCODE) { exit $LASTEXITCODE }
dotnet build .\FinancePro.sln --configuration Release --no-restore; if ($LASTEXITCODE) { exit $LASTEXITCODE }
dotnet build .\FinancePro.Bootstrap\FinancePro.Bootstrap.csproj --configuration Release; if ($LASTEXITCODE) { exit $LASTEXITCODE }
dotnet test .\FinancePro.sln --configuration Release --no-build
exit $LASTEXITCODE
