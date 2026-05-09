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
        BrowserViewport.cs
        BrowserViewports.cs
        PnhPageTest.cs
      Core/
        Configuration/
          AutomationSettingsTests.cs
      Ui/
        PublicHomeSmokeTests.cs
      PnhAutomation.Tests.csproj
  Directory.Build.props
  config/
    test-run.runsettings
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
| `TestCategories` | Central list of category names used by xUnit traits and `TestCaseFilter` values in `config/test-run.runsettings`. |
| `BrowserViewport` | Named browser viewport value with width, height, and conversion helpers for Playwright context and video options. |
| `BrowserViewports` | Shared viewport catalog. Current presets are `DesktopSmall` (`1280x720`) and `MobilePhone` (`390x844`). |
| `PnhPageTest` | Shared Playwright xUnit base class for UI tests. It applies the configured base URL, starts tracing, captures a failure screenshot, and keeps browser video only when a test fails. |
| `PublicHomeSmokeTests` | First production-safe UI smoke tests. They check homepage load, primary navigation availability, and desktop/mobile viewport fit. |

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

The project uses `config/test-run.runsettings` as the default XML test run configuration. Edit that file first, then run `dotnet test`.

The default filter runs unit tests plus production-safe read-only smoke tests:

```xml
<TestCaseFilter>(Category=Unit)|(Category=Smoke&amp;Category=ProductionSafe&amp;Category=ReadOnly)</TestCaseFilter>
```

The first shared configuration values are provided through `RunConfiguration/EnvironmentVariables` in the XML file:

| XML environment variable | Default | Purpose |
|---|---|---|
| `PNH_BASE_URL` | `https://www.pilkanahali.pl/` | Target site URL. |
| `PNH_ENVIRONMENT` | `Production` | Human-readable environment name used by tests and reports. |
| `PNH_ARTIFACT_DIR` | `TestResults/playwright` | Optional folder for failed browser traces, screenshots, and videos. |

The same XML file also controls:

- `RunConfiguration/TestCaseFilter` for selecting which tests run.
- `Playwright/BrowserName` for choosing `chromium`, `firefox`, or `webkit`.
- `Playwright/LaunchOptions/Headless` for headless or visible browser execution.
- `Playwright/LaunchOptions/Channel` for Chrome or Edge when using Chromium.

## Commands

Run from the repository root:

```powershell
dotnet restore
dotnet build
dotnet test
```

Choose the active test set by editing `RunConfiguration/TestCaseFilter` in `config/test-run.runsettings`.

Playwright browser setup:

```powershell
dotnet build tests/PnhAutomation.Tests/PnhAutomation.Tests.csproj
pwsh ./tests/PnhAutomation.Tests/bin/Debug/net10.0/playwright.ps1 install chrome
```

Windows PowerShell fallback:

```powershell
powershell -ExecutionPolicy Bypass -File .\tests\PnhAutomation.Tests\bin\Debug\net10.0\playwright.ps1 install chrome
```

Browser test commands:

```powershell
dotnet test
dotnet test --settings .\config\test-run.runsettings
```

`config/test-run.runsettings` configures Chromium to launch through the Chrome channel by default because the public site can block Playwright's default headless shell. Change `Headless` to `false` in that file to watch browser execution.

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
