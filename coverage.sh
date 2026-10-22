set -e

rm -rf MsTestResults Coverlet-html
dotnet test --configuration Release \
    --no-build \
    --results-directory MsTestResults \
    --coverage \
    --coverage-output-format cobertura
reportgenerator -reports:"MsTestResults/*.cobertura.xml" \
    -reporttypes:"Html" \
    -targetdir:Coverlet-html
