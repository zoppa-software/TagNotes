$resultDirectory = ".\TestResults"
$reportsDirectory = ".\TestReports"

if (Test-Path $resultDirectory) { Remove-Item $resultDirectory -Recurse }
if (Test-Path $reportsDirectory) { Remove-Item $reportsDirectory -Recurse }

dotnet test --no-build --collect:"XPlat Code Coverage" --results-directory $resultDirectory

$xmlFileName = (Get-ChildItem $resultDirectory -Filter *.xml -Recurse -File)[0].FullName

reportgenerator -reports:$xmlFileName -targetdir:$reportsDirectory -reporttypes:Html