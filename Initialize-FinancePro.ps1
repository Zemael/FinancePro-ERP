param(
 [string]$Company = "FinancePro",
 [string]$AdminName = "Administrador FinancePro",
 [string]$AdminEmail = "admin@financepro.local",
 [SecureString]$AdminPassword
)
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
if (-not $AdminPassword) { $AdminPassword = Read-Host "Senha inicial do administrador (mín. 8 caracteres)" -AsSecureString }
$ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($AdminPassword)
try { $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) } finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
if ($plain.Length -lt 8) { throw "A senha deve ter pelo menos 8 caracteres." }
dotnet ef database update --project .\FinancePro.Data\FinancePro.Data.csproj --startup-project .\FinancePro.Data\FinancePro.Data.csproj --configuration Release
if ($LASTEXITCODE) { exit $LASTEXITCODE }
dotnet run --project .\FinancePro.Bootstrap\FinancePro.Bootstrap.csproj --configuration Release -- --company $Company --admin-name $AdminName --admin-email $AdminEmail --admin-password $plain
exit $LASTEXITCODE

