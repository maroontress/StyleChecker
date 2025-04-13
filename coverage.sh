set -e

rm -rf MsTestResults Coverlet-html
dotnet test --collect:"XPlat Code Coverage" \
    --settings coverlet.runsettings.xml \
    --results-directory MsTestResults
reportgenerator -reports:MsTestResults/*/coverage.cobertura.xml \
    -targetdir:Coverlet-html
