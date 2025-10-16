#!/bin/bash
# Run tests with coverage and generate HTML report
# Usage: ./run-coverage.sh

echo "========================================"
echo "Running Tests with Coverage"
echo "========================================"
echo ""

# Clean previous coverage results
echo "Cleaning previous coverage results..."
rm -rf coverage-report
rm -rf TestResults

# Run tests with coverage
echo ""
echo "Running tests with coverage collection..."
dotnet test \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=../TestResults/coverage.cobertura.xml \
    /p:ExcludeByFile="**/Migrations/**/*.cs" \
    --logger "console;verbosity=minimal"

if [ $? -ne 0 ]; then
    echo ""
    echo "Tests failed! Fix the tests before generating coverage report."
    exit 1
fi

# Find coverage file
COVERAGE_FILE="TestResults/coverage.cobertura.xml"
if [ ! -f "$COVERAGE_FILE" ]; then
    echo ""
    echo "Coverage file not found at $COVERAGE_FILE"
    echo "Searching for coverage files..."
    COVERAGE_FILE=$(find . -name "coverage.cobertura.xml" | head -n 1)
    if [ -z "$COVERAGE_FILE" ]; then
        echo "No coverage file found!"
        exit 1
    fi
    echo "Found: $COVERAGE_FILE"
fi

# Generate HTML report
echo ""
echo "Generating HTML coverage report..."
reportgenerator \
    "-reports:$COVERAGE_FILE" \
    "-targetdir:coverage-report" \
    "-reporttypes:Html;HtmlSummary" \
    "-title:Product Catalog API - Coverage Report"

if [ $? -eq 0 ]; then
    echo ""
    echo "========================================"
    echo "Coverage Report Generated Successfully!"
    echo "========================================"
    echo ""
    echo "Open the report: coverage-report/index.html"
    echo ""

    # Try to open the report in browser (Linux/Mac)
    REPORT_PATH="coverage-report/index.html"
    if [ -f "$REPORT_PATH" ]; then
        if command -v xdg-open > /dev/null; then
            xdg-open "$REPORT_PATH"
        elif command -v open > /dev/null; then
            open "$REPORT_PATH"
        fi
    fi
else
    echo ""
    echo "Failed to generate coverage report!"
    exit 1
fi
