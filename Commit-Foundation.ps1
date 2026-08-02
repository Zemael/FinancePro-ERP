$ErrorActionPreference = 'Stop'

$branch = (git branch --show-current).Trim()
if ($branch -ne 'feature/foundation') {
    throw "Branch atual: '$branch'. Mude para feature/foundation antes de continuar."
}

dotnet restore .\FinancePro.sln
dotnet build .\FinancePro.sln --configuration Release --no-restore
dotnet test .\FinancePro.sln --configuration Release --no-build

git add .
if ((git status --porcelain).Count -eq 0) {
    Write-Host 'Não existem alterações para commit.'
    exit 0
}

git commit -m "chore: complete engineering foundation v2.0.2"
git push -u origin feature/foundation
Write-Host 'Foundation enviada. Abra o Pull Request feature/foundation -> develop.'
