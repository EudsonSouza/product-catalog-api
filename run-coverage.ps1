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
dotnet test `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=cobertura `
    /p:CoverletOutput=../TestResults/coverage.cobertura.xml `
    /p:ExcludeByFile="**/Migrations/**/*.cs" `
    --logger "console;verbosity=minimal"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Tests failed! Fix the tests before generating coverage report." -ForegroundColor Red
    exit $LASTEXITCODE
}

# Check if coverage file exists
$coverageFile = "TestResults/coverage.cobertura.xml"
if (-not (Test-Path $coverageFile)) {
    Write-Host ""
    Write-Host "Coverage file not found at $coverageFile" -ForegroundColor Red
    Write-Host "Searching for coverage files..." -ForegroundColor Yellow
    Get-ChildItem -Recurse -Filter "coverage.cobertura.xml" | ForEach-Object {
        Write-Host "Found: $($_.FullName)" -ForegroundColor Green
        $coverageFile = $_.FullName
    }
}

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
