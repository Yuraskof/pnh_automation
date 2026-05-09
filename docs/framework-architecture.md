# Framework Architecture

Last reviewed: 2026-04-25

This document describes the current test solution structure. The project has moved from a console skeleton to a test-oriented layout with a shared core library and a test project.

## Solution Layout

```text
pnh_automation/
  src/
    PnhAutomation.Core/
      Configuration/
        AutomationSettings.cs
      Testing/
        TestCategories.cs
      PnhAutomation.Core.csproj
  tests/
    PnhAutomation.Tests/
      Browser/
        PnhPageTest.cs
      Core/
        Configuration/
          AutomationSettingsTests.cs
      PnhAutomation.Tests.csproj
  Directory.Build.props
  Dockerfile
  compose.yaml
  pnh_automation.sln
```

## Project Responsibilities

| Project | Responsibility |
|---|---|
| `PnhAutomation.Core` | Shared test framework code that should not depend on a specific test runner. This is where configuration, category names, future page models, API clients, test data builders, and production-safety helpers should live. |
| `PnhAutomation.Tests` | xUnit test project. This is where test scenarios and test-runner-specific fixtures live. It references `PnhAutomation.Core`, uses FluentAssertions for readable assertions, and uses Playwright for browser automation. |

## Current Core Types

| Type | Purpose |
|---|---|
| `AutomationSettings` | Holds the target base URL and environment name. Defaults to the public production URL and can read `PNH_BASE_URL` and `PNH_ENVIRONMENT` from the current process or an explicit dictionary. |
| `TestCategories` | Central list of category names used by xUnit traits and `dotnet test --filter` commands. |
| `PnhPageTest` | Shared Playwright xUnit base class for UI tests. It applies the configured base URL, starts tracing, captures a failure screenshot, and keeps browser video only when a test fails. |

Example:

```csharp
[Trait("Category", TestCategories.Unit)]
public void AutomationSettings_CreatesProductionDefaults_UsesPublicBaseUrl()
{
    var settings = AutomationSettings.ProductionDefaults();

    settings.BaseUrl.Should().Be(new Uri("https://www.pilkanahali.pl/"));
}
```

## Configuration

The first shared configuration values are:

| Environment variable | Default | Purpose |
|---|---|---|
| `PNH_BASE_URL` | `https://www.pilkanahali.pl/` | Target site URL. |
| `PNH_ENVIRONMENT` | `Production` | Human-readable environment name used by tests and reports. |
| `PNH_ARTIFACT_DIR` | `TestResults/playwright` | Optional folder for failed browser traces, screenshots, and videos. |

PowerShell example:

```powershell
$env:PNH_BASE_URL = "https://www.pilkanahali.pl/"
$env:PNH_ENVIRONMENT = "Production"
dotnet test
```

## Commands

Run from the repository root:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet test --filter "Category=Unit"
```

Playwright browser setup:

```powershell
dotnet build tests/PnhAutomation.Tests/PnhAutomation.Tests.csproj
pwsh ./tests/PnhAutomation.Tests/bin/Debug/net10.0/playwright.ps1 install
```

Browser test commands:

```powershell
$env:PNH_BASE_URL = "https://www.pilkanahali.pl/"
dotnet test --filter "Category=Ui"
dotnet test --filter "Category=Ui&Category=Smoke&Category=ProductionSafe"
```

Playwright run options:

```powershell
$env:HEADED = "1"
$env:BROWSER = "chromium"
dotnet test --filter "Category=Ui"
```

Failed browser tests write traces, screenshots, and video to `TestResults/playwright` unless `PNH_ARTIFACT_DIR` points somewhere else. Open a failed trace with:

```powershell
pwsh ./tests/PnhAutomation.Tests/bin/Debug/net10.0/playwright.ps1 show-trace ./TestResults/playwright/<TestClass>/<RunId>/trace.zip
```

Docker test runner:

```powershell
docker compose build
docker compose run --rm pnh_automation
```

The Docker service uses the root `Dockerfile`. It restores the solution during image build and runs:

```powershell
dotnet test pnh_automation.sln --no-restore
```

## Next Architecture Steps

Planned additions:

- Page and component objects for stable UI automation.
- API clients for read-only contract checks.
- Test data builders that keep production data synthetic.
- Report output with safe artifact handling.
