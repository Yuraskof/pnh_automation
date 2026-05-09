# Browser Automation

## Purpose

Browser tests use Playwright to open the real site in a real browser. In simple words: unit tests check small pieces of code, while browser tests check what a user can see and do on the page.

Because the default target is live production, browser tests must stay low-volume and production-safe. They should read public information, use a dedicated test account when authentication is needed, and stop before booking, payment, prepaid, notification, or organizer side effects.

## Install Browsers

Run these commands from the repository root after restoring packages:

```powershell
dotnet restore
dotnet build tests/PnhAutomation.Tests/PnhAutomation.Tests.csproj
pwsh ./tests/PnhAutomation.Tests/bin/Debug/net10.0/playwright.ps1 install chrome
```

If `pwsh` is not installed on Windows, use Windows PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File .\tests\PnhAutomation.Tests\bin\Debug\net10.0\playwright.ps1 install chrome
```

On a Linux CI runner or a fresh Linux machine, install Playwright system dependencies too:

```powershell
dotnet build tests/PnhAutomation.Tests/PnhAutomation.Tests.csproj
pwsh ./tests/PnhAutomation.Tests/bin/Debug/net10.0/playwright.ps1 install --with-deps chrome
```

The `dotnet build` step matters because Playwright creates `playwright.ps1` inside the test project's `bin` folder during build.

The project uses `config/test-run.runsettings` as the editable XML test run configuration. In simple words: choose the browser, visible/headless mode, target URL, debug mode, and test filter in that file, then run one command: `dotnet test`.

## XML Run Config

Edit this file before running tests:

```text
config/test-run.runsettings
```

All common choices are visible in XML comments. Keep one active value per setting.

Important settings:

| XML setting | Purpose |
|---|---|
| `RunConfiguration/TestCaseFilter` | Chooses which tests run. |
| `RunConfiguration/EnvironmentVariables/PNH_BASE_URL` | Site opened by Playwright when a test uses relative paths like `/`. |
| `RunConfiguration/EnvironmentVariables/PNH_ENVIRONMENT` | Human-readable environment name for reports and diagnostics. |
| `RunConfiguration/EnvironmentVariables/PNH_ARTIFACT_DIR` | Optional folder for failed browser traces, screenshots, and videos. |
| `RunConfiguration/EnvironmentVariables/PWDEBUG` | Opens Playwright Inspector when uncommented. |
| `Playwright/BrowserName` | Chooses `chromium`, `firefox`, or `webkit`. |
| `Playwright/LaunchOptions/Headless` | `true` runs in the background; `false` opens a visible browser. |
| `Playwright/LaunchOptions/Channel` | Chooses Chrome or Edge when `BrowserName` is `chromium`. |

Example: run homepage smoke tests in a visible Chrome window:

```xml
<TestCaseFilter>FullyQualifiedName~PublicHomeSmokeTests</TestCaseFilter>
<BrowserName>chromium</BrowserName>
<Headless>false</Headless>
<Channel>chrome</Channel>
```

If `PNH_BASE_URL` is changed to a staging site, make sure the test categories match the target. Tests that can change data must not run against production.

## Run Configured Tests

After editing `config/test-run.runsettings`, run:

```powershell
dotnet test
```

The default active filter runs unit tests and production-safe read-only smoke tests:

```xml
<TestCaseFilter>(Category=Unit)|(Category=Smoke&amp;Category=ProductionSafe&amp;Category=ReadOnly)</TestCaseFilter>
```

To run from an explicit settings file path:

```powershell
dotnet test --settings .\config\test-run.runsettings
```

Use these filter values inside `TestCaseFilter`:

| Goal | `TestCaseFilter` value |
|---|---|
| Unit tests | `Category=Unit` |
| All UI tests | `Category=Ui` |
| Production-safe UI smoke tests | `Category=Ui&Category=Smoke&Category=ProductionSafe` |
| Homepage smoke class | `FullyQualifiedName~PublicHomeSmokeTests` |

In XML, `&` must be written as `&amp;`, so the production-safe smoke filter is stored as `Category=Ui&amp;Category=Smoke&amp;Category=ProductionSafe`.

In simple words: `Headless=false` lets you watch the browser, while uncommenting `PWDEBUG` opens Playwright's debugging tools so you can pause and inspect what the test sees.

## Docker Browser Runner

Docker uses the repository `Dockerfile` and `compose.yaml`:

```powershell
docker compose build
docker compose run --rm pnh_automation
```

The Dockerfile starts from the official Playwright .NET image for the same Playwright version used by the test project. That image contains Playwright browser dependencies. The Docker build also installs the .NET 10 SDK when needed and installs the Chrome browser channel used by the runsettings file.

In simple words: Docker should behave like a clean machine that already knows how to launch browsers for Playwright tests.

## Test Base Class

New browser tests should inherit `PnhPageTest`. The base class creates a Playwright context with:

- `BaseURL` set from `PNH_BASE_URL`.
- A stable desktop viewport from `BrowserViewports.DesktopSmall`.
- Trace recording with screenshots, snapshots, and source files.
- A full-page `failure.png` screenshot when the test fails.
- Video recording during the test, kept only when the test fails.

Example:

```csharp
using System.Text.RegularExpressions;
using PnhAutomation.Core.Testing;
using PnhAutomation.Tests.Browser;

namespace PnhAutomation.Tests.Ui;

public sealed class HomePageSmokeTests : PnhPageTest
{
    [Fact]
    [Trait("Category", TestCategories.Ui)]
    [Trait("Category", TestCategories.Smoke)]
    [Trait("Category", TestCategories.ProductionSafe)]
    [Trait("Category", TestCategories.ReadOnly)]
    public async Task AnonymousUser_CanOpenHomePage()
    {
        await Page.GotoAsync("/");

        await Expect(Page).ToHaveTitleAsync(new Regex("Hali", RegexOptions.IgnoreCase));
    }
}
```

## Viewport Catalog

Viewport presets live in `tests/PnhAutomation.Tests/Browser/BrowserViewports.cs`.

Current presets:

| Name | Size | Purpose |
|---|---:|---|
| `DesktopSmall` | `1280x720` | Default browser context and desktop smoke coverage. |
| `MobilePhone` | `390x844` | Mobile smoke coverage. |

In simple words: tests should use names like `BrowserViewports.DesktopSmall` instead of repeating raw numbers like `1280` and `720`. This keeps the intent clear and makes future viewport changes happen in one place.

## Locator Preference

Current UI tests should prefer relative XPath locators first, using stable and readable XPath expressions.

Preferred order:

1. Relative XPath using best practices.
2. Stable test id, when the application provides one.
3. CSS locator for stable technical structure.
4. Role locator, for example `GetByRole(AriaRole.Link, ...)`.

Good example:

```csharp
Page.Locator("//a[contains(translate(normalize-space(.), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'oferta')]")
```

Avoid brittle absolute XPath:

```csharp
Page.Locator("/html/body/div[1]/header/nav/a[3]")
```

## Current Homepage Smoke Coverage

`PublicHomeSmokeTests` automates the first live-production UI checks:

| Test | What it checks | Why it is safe |
|---|---|---|
| `AnonymousUser_OpensPublicHomePage_AppLoads` | The public homepage returns a successful response, is not the block page, has title `PNH`, and renders the hero heading. | It only opens the public page. |
| `AnonymousUser_ViewsPublicHomePage_PrimaryNavigationIsAvailable` | The top navigation exposes links for finding classes, adding classes, offer, and how it works. | It checks link availability without submitting forms or changing data. |
| `AnonymousUser_OpensPublicHomePage_LayoutFitsViewport` | The homepage fits desktop `1280x720` and mobile `390x844` viewports without horizontal overflow. | It only resizes the local browser viewport and reads page layout. |

## Failure Artifacts

Failed browser tests keep artifacts under:

```text
TestResults/playwright/<TestClass>/<RunId>/
```

The folder contains:

| File | Meaning |
|---|---|
| `trace.zip` | Step-by-step Playwright trace with DOM snapshots and action screenshots. |
| `failure.png` | Full-page screenshot captured during test cleanup. |
| `videos/*.webm` | Browser video kept only for failed tests. |

Open a trace locally:

```powershell
pwsh ./tests/PnhAutomation.Tests/bin/Debug/net10.0/playwright.ps1 show-trace ./TestResults/playwright/<TestClass>/<RunId>/trace.zip
```

Do not commit `TestResults`. Traces, screenshots, and videos can contain page content, cookies, tokens, names, emails, or other personal data.
