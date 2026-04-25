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
| `PnhAutomation.Tests` | xUnit test project. This is where test scenarios live. It references `PnhAutomation.Core` and uses FluentAssertions for readable assertions. |

## Current Core Types

| Type | Purpose |
|---|---|
| `AutomationSettings` | Holds the target base URL and environment name. Defaults to the public production URL and can read `PNH_BASE_URL` and `PNH_ENVIRONMENT` from the current process or an explicit dictionary. |
| `TestCategories` | Central list of category names used by xUnit traits and `dotnet test --filter` commands. |

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

- Playwright browser dependencies and browser install command.
- Page and component objects for stable UI automation.
- Test fixtures for browser, context, tracing, screenshots, and environment settings.
- API clients for read-only contract checks.
- Test data builders that keep production data synthetic.
- Report output with safe artifact handling.
