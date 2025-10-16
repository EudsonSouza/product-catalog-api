---
title: Code Coverage Guide
last_updated: 2025-10-16
source: migrated
---

# Code Coverage Guide

This guide explains how to run tests with code coverage and generate HTML reports for the Product Catalog API.

## Prerequisites

- .NET 9.0 SDK installed
- ReportGenerator tool (installed globally)

## Quick Start

### Windows (PowerShell)
```powershell
.\run-coverage.ps1
```

### Linux/Mac (Bash)
```bash
./run-coverage.sh
```

These scripts will:
1. Clean previous coverage results
2. Run all tests with coverage collection
3. Generate an HTML coverage report
4. Automatically open the report in your browser

## Manual Coverage Generation

If you prefer to run commands manually:

### Step 1: Run Tests with Coverage
```bash
dotnet test ProductCatalog.sln --collect:"XPlat Code Coverage"
```

### Step 2: Generate HTML Report
```bash
reportgenerator \
  "-reports:tests/**/coverage.cobertura.xml" \
  "-targetdir:coverage-report" \
  "-reporttypes:Html;HtmlSummary" \
  "-title:Product Catalog API - Coverage Report"
```

### Step 3: View Report
Open `coverage-report/index.html` in your browser.

## Installing ReportGenerator

If you haven't installed ReportGenerator yet:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

## Coverage Report Location

After running the coverage scripts, the HTML report will be generated in:
```
product-catalog-api/coverage-report/index.html
```

## Understanding the Coverage Report

The coverage report shows:

- **Line Coverage**: Percentage of code lines executed during tests
- **Branch Coverage**: Percentage of conditional branches (if/else) tested
- **Method Coverage**: Percentage of methods called during tests

### Coverage Targets (Phase 1 Goals)

| Layer | Target Coverage |
|-------|----------------|
| Domain | >90% |
| Services | >90% |
| Data | >85% |
| API/Controllers | >80% |
| **Overall** | **>80%** |

## Current Coverage Status

Run the coverage scripts to see the current status. The report will show:

- Summary by project/namespace
- Detailed line-by-line coverage for each file
- Uncovered lines highlighted in red
- Partially covered lines in yellow
- Fully covered lines in green

## Excluding Files from Coverage

Certain files are automatically excluded from coverage analysis:

- Migrations (`**/Migrations/**/*.cs`)
- Auto-generated files
- Configuration classes (debatable - can be included)

## CI/CD Integration

This project uses GitHub Actions to automatically run tests and update the coverage badge on every push to the master branch.

### Dynamic Coverage Badge

The coverage badge in the README updates automatically using the [Dynamic Badges Action](https://github.com/marketplace/actions/dynamic-badges).

**Badge URL:**
```
https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/EudsonSouza/8cdc218770b5707458a6c377a1612355/raw/coverage.json
```

**Badge Colors:**
- 🟢 Green: Coverage ≥ 80% (Target achieved!)
- 🟡 Yellow: Coverage 50-79% (Needs improvement)
- 🔴 Red: Coverage < 50% (Critical - needs immediate attention)

### GitHub Actions Workflow

The project includes a `.github/workflows/test-coverage.yml` workflow that:

1. Runs on every push to `master` or `develop` branches
2. Runs on every pull request
3. Executes all tests with coverage collection
4. Generates a coverage report
5. Extracts the coverage percentage
6. Updates the badge in GitHub Gist (only on master branch pushes)
7. Comments the coverage percentage on pull requests
8. Uploads the full coverage report as an artifact (retained for 30 days)

**Workflow file location:** `.github/workflows/test-coverage.yml`

### How It Works

1. **Test Execution**: Tests run with `dotnet test --collect:"XPlat Code Coverage"`
2. **Report Generation**: ReportGenerator creates a text summary of coverage
3. **Percentage Extraction**: A script extracts the line coverage percentage
4. **Badge Update**: The Dynamic Badges Action updates a JSON file in a GitHub Gist
5. **Badge Display**: Shields.io renders the badge from the Gist data

### Setting Up for Your Repository

If you fork this repository, you'll need to:

1. Create a public GitHub Gist
2. Create a Personal Access Token with `gist` scope
3. Add the token as a repository secret named `GIST_SECRET`
4. Update the `gistID` in `.github/workflows/test-coverage.yml`
5. Update the badge URL in `README.md`

### Viewing Coverage Reports

**On GitHub Actions:**
- Go to Actions → Test Coverage workflow
- Click on a workflow run
- Download the "coverage-report" artifact
- Extract and open `index.html`

**Locally:**
Run `./run-coverage.ps1` (Windows) or `./run-coverage.sh` (Linux/Mac)

## Troubleshooting

### Issue: "reportgenerator: command not found"
**Solution**: Install ReportGenerator globally:
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Issue: No coverage files found
**Solution**: Make sure tests ran successfully first:
```bash
dotnet test ProductCatalog.sln
```

### Issue: Coverage shows 0% for all files
**Solution**: Check that:
1. Tests are actually running and passing
2. The test projects reference the source projects correctly
3. You're using the correct coverage collector (`--collect:"XPlat Code Coverage"`)

## Useful Commands

### Run only unit tests
```bash
dotnet test tests/ProductCatalog.Tests.Unit/ProductCatalog.Tests.Unit.csproj --collect:"XPlat Code Coverage"
```

### Run only integration tests
```bash
dotnet test tests/ProductCatalog.Tests.Integration/ProductCatalog.Tests.Integration.csproj --collect:"XPlat Code Coverage"
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~ProductTests"
```

### Run tests in watch mode (no coverage)
```bash
dotnet watch test
```

## Coverage Best Practices

1. **Focus on business logic**: Prioritize testing domain entities and services
2. **Don't obsess over 100%**: 80-90% coverage is generally sufficient
3. **Test behavior, not implementation**: Coverage is a metric, not a goal
4. **Use coverage to find gaps**: Look for untested edge cases and error paths
5. **Exclude noise**: Don't include migrations, generated code, or trivial DTOs

## Related Documentation

- [Testing Strategy](./Tests.md)
- [Phase 1 Roadmap](../04-Governance/Roadmap.md)
- [Current Work](../06-Current%20Work/current_work.md)

---

**Last Updated**: 2025-10-16
**Current Coverage**: Run `./run-coverage.ps1` or `./run-coverage.sh` to see latest results
