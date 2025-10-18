# Run tests with coverage and generate HTML report
# Usage: .\run-coverage.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Running Tests with Coverage" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Clean previous coverage results
Write-Host "Cleaning previous coverage results..." -ForegroundColor Yellow
if (Test-Path "coverage-report") {
    Remove-Item -Recurse -Force "coverage-report"
}
if (Test-Path "TestResults") {
    Remove-Item -Recurse -Force "TestResults"
}

# Run tests with coverage
Write-Host ""
Write-Host "Running tests with coverage collection..." -ForegroundColor Yellow

# Run unit tests with coverage
dotnet test tests/ProductCatalog.Tests.Unit/ProductCatalog.Tests.Unit.csproj `
    --collect:"XPlat Code Coverage" `
    --results-directory:"./TestResults" `
    --settings:"coverage.runsettings" `
    --logger "console;verbosity=minimal"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Tests failed! Fix the tests before generating coverage report." -ForegroundColor Red
    exit $LASTEXITCODE
}

# Find coverage file
Write-Host ""
Write-Host "Searching for coverage files..." -ForegroundColor Yellow
$coverageFile = Get-ChildItem -Path "TestResults" -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1 -ExpandProperty FullName

if (-not $coverageFile) {
    Write-Host "No coverage.cobertura.xml found. Trying to find any coverage file..." -ForegroundColor Yellow
    $coverageFile = Get-ChildItem -Path "TestResults" -Recurse -Filter "*.cobertura.xml" | Select-Object -First 1 -ExpandProperty FullName
}

if (-not $coverageFile) {
    Write-Host "ERROR: No coverage file found in TestResults!" -ForegroundColor Red
    Write-Host "Contents of TestResults:" -ForegroundColor Yellow
    Get-ChildItem -Path "TestResults" -Recurse | ForEach-Object { Write-Host $_.FullName }
    exit 1
}

Write-Host "Using coverage file: $coverageFile" -ForegroundColor Green

# Generate HTML report
Write-Host ""
Write-Host "Generating HTML coverage report..." -ForegroundColor Yellow
reportgenerator `
    "-reports:$coverageFile" `
    "-targetdir:coverage-report" `
    "-reporttypes:Html;HtmlSummary" `
    "-title:Product Catalog API - Coverage Report"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Coverage Report Generated Successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Open the report: coverage-report\index.html" -ForegroundColor Cyan
    Write-Host ""

    # Try to open the report in browser
    $reportPath = Join-Path (Get-Location) "coverage-report\index.html"
    if (Test-Path $reportPath) {
        Write-Host "Opening coverage report in browser..." -ForegroundColor Yellow
        Start-Process $reportPath
    }
} else {
    Write-Host ""
    Write-Host "Failed to generate coverage report!" -ForegroundColor Red
    exit $LASTEXITCODE
}
